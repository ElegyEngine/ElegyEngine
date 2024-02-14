// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ConsoleCommands
{
	/// <summary>
	/// Provides utilities for static ConVars and ConCommands.
	/// </summary>
	internal class ConVarRegistry
	{
		private const BindingFlags BaseBindingFlags =
					BindingFlags.DeclaredOnly |
					BindingFlags.Public |
					BindingFlags.NonPublic;

		private const BindingFlags StaticBindingFlags =
					BaseBindingFlags | BindingFlags.Static;

		private const BindingFlags InstanceBindingFlags =
					BaseBindingFlags | BindingFlags.Instance;

		public List<ConsoleCommand> Commands { get; } = new();

		public ConVarRegistry( Assembly? assembly = null, IPlugin? instance = null )
		{
			if ( assembly is not null )
			{
				var types = assembly.GetTypes();
				for ( int i = 0; i < types.Length; i++ )
				{
					var type = types[i];

					// 1. ConsoleCommand members
					var staticMembers = type.GetFields( StaticBindingFlags );
					foreach ( var member in staticMembers )
					{
						if ( member.FieldType == typeof( ConsoleCommand ) )
						{
							ConsoleCommand? cmd = member.GetValue( null ) as ConsoleCommand;
							Commands.Add( cmd );
						}
					}

					// 2. Methods with ConsoleCommandAttribute
					var staticMethods = type.GetMethods( StaticBindingFlags );
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

			// 3. Instance methods
			if ( instance is not null )
			{
				var instanceMethods = instance.GetType().GetMethods( InstanceBindingFlags );
				foreach ( var method in instanceMethods )
				{
					ConsoleCommandAttribute? attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
					if ( attribute is null )
					{
						continue;
					}

					ConsoleCommand? consoleCommand = ConsoleCommand.FromMethod( method, attribute, instance );
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
