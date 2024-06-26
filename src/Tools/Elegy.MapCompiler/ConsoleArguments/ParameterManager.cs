﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Reflection;
using Elegy.MapCompiler.Assets;

namespace Elegy.MapCompiler.ConsoleArguments
{
	public static class ParameterManager
	{
		// Type information for MapCompilerParameters is cached here for faster access, though
		// it doesn't make any difference here
		private readonly static TypeInfo mParametersTypeInfo = TypeInfo.From<MapCompilerParameters>();

		public static bool ProcessSingleArgument( string name, string value, MapCompilerParameters mapParameters )
		{
			// 1. Find the corresponding attribute object
			CompilerParamAttribute? argumentObject = null;
			PropertyWrapper? propertyObject = null;
			foreach ( var property in mParametersTypeInfo.Properties )
			{
				foreach ( var propertyAttribute in property.Attributes )
				{
					argumentObject = propertyAttribute as CompilerParamAttribute;
					if ( argumentObject?.Name == name )
					{
						propertyObject = property;
						break;
					}
				}

				if ( propertyObject is not null )
				{
					break;
				}
			}

			if ( propertyObject is null || argumentObject is null )
			{
				return false;
			}

			// 2. Try parsing it
			object argumentValue;
			try
			{
				argumentValue = argumentObject.Parse( value );
			}
			catch ( Exception )
			{
				Console.Error( "Elegy.MapCompiler", $"Parameter '{argumentObject.Name}' has invalid value '{value}'" );
				Console.Error( "Elegy.MapCompiler", $"This parameter can only be {argumentObject.GetPossibleValues()}" );
				return true;
			}

			// 3. Try setting it
			try
			{
				propertyObject.Info.SetValue( mapParameters, argumentValue );
			}
			catch ( Exception ex )
			{
				Console.Error( "Elegy.MapCompiler", $"Internal error: failed to set parameter '{argumentObject.Name}'" );
				Console.Error( "Elegy.MapCompiler", $"Message: {ex.Message}" );
			}

			return true;
		}

		public static void ProcessArguments( string[] args, out MapCompilerParameters parameters )
		{
			parameters = new();
			for ( int i = 0; i < args.Length; i++ )
			{
				string argumentName = args[i];
				string argumentValue = "1";
				if ( i < args.Length - 1 )
				{
					argumentValue = args[i + 1];
					i++;
				}

				if ( !ProcessSingleArgument( argumentName, argumentValue, parameters ) )
				{
					Console.Error( "Elegy.MapCompiler", $"Unknown parameter '{argumentName}' with value '{argumentValue}'" );
				}
			}
		}
	}
}
