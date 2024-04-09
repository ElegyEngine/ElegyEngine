// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;
using System.Reflection;
using System.Runtime.Loader;

using Console = Elegy.ConsoleSystem.API.Console;

namespace Elegy.PluginSystem
{
	internal class PluginLoadContext : AssemblyLoadContext
	{
		private List<(string, Assembly)> mDependencies = new(); 

		public PluginLoadContext()
			: base( isCollectible: true )
		{
			RegisterDependency( "Elegy.Common",			typeof( Common.Text.Lexer ).Assembly );
			RegisterDependency( "Elegy.ConsoleSystem",	typeof( ConsoleSystem.API.Console ).Assembly );
			RegisterDependency( "Elegy.FileSystem",		typeof( FileSystem.API.Files ).Assembly );
			RegisterDependency( "Elegy.PluginSystem",	typeof( PluginSystem.API.Plugins ).Assembly );
		}

		public void RegisterDependency( string name, Assembly assembly )
		{
			if ( mDependencies.Any( ( pair ) => pair.Item1 == name ) )
			{
				Console.Warning( "DllLoader", $"Tried registering an existing dependency '{name}.dll'" );
				return;
			}

			mDependencies.Add( (name, assembly) );
		}

		protected override Assembly? Load( AssemblyName assemblyName )
		{
			Console.Log( "DllLoader", $"'{assemblyName.Name}.dll'", ConsoleMessageType.Verbose );

			foreach ( var dependency in mDependencies )
			{
				if ( dependency.Item1 == assemblyName.Name )
				{
					return dependency.Item2;
				}
			}

			// Fallback to the host context
			return null;
		}
	}
}
