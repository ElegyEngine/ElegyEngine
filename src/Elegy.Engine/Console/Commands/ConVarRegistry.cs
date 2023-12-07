// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.ConsoleCommands.Helpers;

namespace Elegy.ConsoleCommands
{
	/// <summary>
	/// Provides utilities for static ConVars and ConCommands.
	/// </summary>
	internal class ConVarRegistry
	{
		private const BindingFlags ConsoleBindingFlags = 
					BindingFlags.DeclaredOnly |
					BindingFlags.Static |
					BindingFlags.Public |
					BindingFlags.NonPublic;

		public List<ConsoleCommand> Commands { get; } = new();

		public ConVarRegistry( Assembly assembly )
		{
			var types = assembly.GetTypes();
			for ( int i = 0; i < types.Length; i++ )
			{
				var type = types[i];

				// 1. ConsoleCommand members
				var staticMembers = type.GetFields( ConsoleBindingFlags );
				foreach ( var member in staticMembers )
				{
					if ( member.FieldType == typeof( ConsoleCommand ) )
					{
						ConsoleCommand? cmd = member.GetValue( null ) as ConsoleCommand;
						Commands.Add( cmd );
					}
				}

				// 2. Methods with ConsoleCommandAttribute
				var staticMethods = type.GetMethods( ConsoleBindingFlags );
				foreach ( var method in staticMethods )
				   {
					ConsoleCommandAttribute? attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
					if ( attribute is null )
					{
						continue;
					}

					ConsoleCommand? consoleCommand = ConsoleCommand.FromMethod( method, attribute );
					if ( consoleCommand is null )
					{
						Console.Warning( "ConVarRegistry", $"Cannot create console command '{attribute.Name}' ({method.DeclaringType.Name}.{method.Name})" );
						continue;
					}

					Commands.Add( consoleCommand );
				}
			}
		}

		public void RegisterAll()
		{
			Commands.ForEach( cmd => Console.RegisterCommand( cmd ) );
		}

		public void UnregisterAll()
		{
			Commands.ForEach( cmd => Console.UnregisterCommand( cmd ) );
		}
	}
}
