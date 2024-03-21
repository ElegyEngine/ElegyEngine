// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

/*
This is one of the more complicated parts of the engine, but I tried to keep the design simple & practical to an extent.
Methods can be console commands so long as they satisfy some basic requirements:
* Returns void or bool
* If it has parametres, they must be any of [int, float, double, bool, string], or string[]
  and these parametres may not have any modifiers: in, out, ref etc.
* Can be static, can be non-static too (private/internal/public is fine)

public void Example( string a, int b, float c ) { ... } <- good
public int Example( in string a, DateTime b, out float c ) { ... } <- bad
*/

/*
From an inner workings POV, this is what's going on in the console system and here:
1) Scan all methods in every class
2) If they have a ConsoleCommand attribute, try creating a ConsoleCommand for them
3) Create helpers particularly for that ConsoleCommand's argument parsing
4) Create autocomplete suggestions, validation etc. with the attached attributes,
   or if that is not available, by inspecting the method
*/

using Elegy.Engine.ConsoleCommands.Helpers;
using Elegy.Engine.Interfaces;
using System.Text;

namespace Elegy.Engine.ConsoleCommands
{
	/// <summary>
	/// Console command.
	/// </summary>
	public partial class ConsoleCommand
	{
		private static AutocompleteMethod GetOrCreateAutocomplete( Dictionary<string, ConsoleParameter> dictionary, MethodInfo method )
		{
			return DefaultAutocomplete;
		}

		private static ValidateMethod GetOrCreateValidate( Dictionary<string, ConsoleParameter> dictionary, MethodInfo method )
		{
			return DefaultValidate;
		}

		private static object?[]? ResolveCommandParametres( string[] args, Dictionary<string, ConsoleParameter> parameters )
		{
			object?[] argumentValues = new object[parameters.Count];
			foreach ( var parameter in parameters.Values )
			{
				argumentValues[parameter.Id] = parameter.DefaultValue;
			}

			// 1st parameter is nameless
			argumentValues[0] = parameters.Values.First().ArgumentHelper.Parse( args[0] );

			// All others are named
			for ( int i = 1; i < args.Length; i++ )
			{
				ReadOnlySpan<char> arg = args[i];
				if ( arg[0] != '-' )
				{
					Console.Warning( "ConsoleCommand", $"Unknown argument '-{arg}'" );
					continue;
				}

				if ( i == args.Length - 1 )
				{
					Console.Warning( "ConsoleCommand", $"Argument '{arg}' has no value after it" );
					continue;
				}

				// Eat the '-'
				arg = arg[1..];

				// Validation is done, we've ensured that all provided arguments are supported and have
				// an appropriate helper, so just go ahead, fetch everything and parse, no checking
				var parameter = parameters[arg.ToString()];
				argumentValues[parameter.Id] = parameter.ArgumentHelper.Parse( args[++i] );
			}

			return argumentValues;
		}

		// The method can be of any signature, we gotta ensure that the string arguments passed in are all tidy
		private static CommandMethod GetOrCreateCommand( Dictionary<string, ConsoleParameter> dictionary, MethodInfo method, IPlugin? instance = null )
		{
			var parametres = method.GetParameters();
			bool hasSimpleParams = parametres.Length == 1 && parametres[0].ParameterType == typeof( string[] );
			bool hasNoParams = parametres.Length == 0;
			bool returnsBool = method.ReturnType == typeof( bool );

			Func<string[], object?> methodInvocation = (hasSimpleParams, hasNoParams) switch
			{
				(true, false) => (args) => method.Invoke( instance, new object[] { args } ),
				(false, true) => (args) => method.Invoke( instance, null ),
				(false, false) => (args) => method.Invoke( instance, ResolveCommandParametres( args, dictionary ) ),
				_ => throw new NotSupportedException() // You can't have both simple and no params
			};

			if ( returnsBool )
			{
				return ( args ) =>
				{
					return (bool?)methodInvocation( args ) ?? false;
				};
			}

			return ( args ) =>
			{
				methodInvocation( args );
				return true;
			};
		}

		/// <summary>
		/// Creates a <see cref="ConsoleCommand"/> from a provided <paramref name="method"/>, whose parametres
		/// must be primitives (<see cref="int"/>, <see cref="float"/> etc.) or <see cref="string"/>.
		/// If <paramref name="instance"/> is not provided, <paramref name="method"/> must be static.
		/// </summary>
		public static ConsoleCommand? FromMethod( MethodInfo method, ConsoleCommandAttribute attribute, IPlugin? instance = null )
		{
			bool isOkay = true;

			if ( instance is null && !method.IsStatic )
			{
				Console.Error( "ConsoleCommand",
					$"Can't register instance method '{method.DeclaringType.Name}.{method.Name}' without an instance of {method.DeclaringType.Name}." );
				isOkay = false;
			}

			bool returnsBool = method.ReturnType == typeof( bool );
			if ( !returnsBool && method.ReturnType != typeof( void ) )
			{
				Console.Error( "ConsoleCommand",
					$"Method '{method.Name}' returns {method.ReturnType.Name} when only bool and void are supported." );
				isOkay = false;
			}

			var parametres = method.GetParameters();
			StringBuilder sb = new( parametres.Length * 2 );
			foreach ( var param in parametres )
			{
				if ( !HelperManager.HasHelperForType( param.ParameterType ) )
				{
					sb.Append( $"\n  * '{param.Name}' is '{param.ParameterType.Name}'" );
				}
				if ( param.IsOut )
				{
					sb.Append( $"\n  * '{param.Name}' is an 'out' parametre" );
				}
				if ( param.ParameterType.IsByRef )
				{
					sb.Append( $"\n  * '{param.Name}' is a 'ref' parametre" );
				}
			}

			if ( sb.Length > 0 )
			{
				Console.Error( "ConsoleCommand", $"Method '{method.Name}'s parametres are not supported:{sb}" );
				sb.Clear();
				foreach ( var helper in HelperManager.Helpers.Values )
				{
					sb.Append( $"\n  * {helper.Type.Name}" );
				}
				Console.Warning( "ConsoleCommand", $"You can use any of the following without 'out', 'ref' and the like:{sb}" );
				isOkay = false;
			}

			if ( !isOkay )
			{
				Console.Warning( "ConsoleCommand", $"Could not register console command '{attribute.Name}' for the reasons above." );
				return null;
			}

			Dictionary<string, ConsoleParameter> dictionary = parametres
				.Select( p => new ConsoleParameter( HelperManager.Helpers[p.ParameterType], p ) )
				.ToDictionary( p => p.Name );

			AutocompleteMethod autocomplete = GetOrCreateAutocomplete( dictionary, method );
			ValidateMethod validate = GetOrCreateValidate( dictionary, method );
			CommandMethod commandMethod = GetOrCreateCommand( dictionary, method, instance );

			return new ConsoleCommand( attribute.Name, commandMethod )
			{
				Autocomplete = autocomplete,
				Validate = validate
			};
		}
	}
}
