// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Extensions;
using Elegy.ConsoleSystem;
using System.Reflection;
using System.Runtime.Loader;

namespace Elegy.PluginSystem
{
	internal class PluginLoadContext : AssemblyLoadContext
	{
		private List<(string, Assembly)> mDependencies = new(); 
		private TaggedLogger mLogger = new( "DllLoader" );

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
				mLogger.Warning( $"Tried registering an existing dependency '{name}.dll'" );
				return;
			}

			mDependencies.Add( (name, assembly) );
		}

		public bool UnregisterDependency( string name )
		{
			int index = mDependencies.FindIndex( pair => pair.Item1 == name );
			if ( index == -1 )
			{
				mLogger.Warning( $"Tried unregistering a non-existing dependency '{name}.dll'" );
				return false;
			}

			mDependencies.RemoveAt( index );
			return true;
		}

		protected override Assembly? Load( AssemblyName assemblyName )
		{
			mLogger.Verbose( $"'{assemblyName.Name}.dll'" );

			// Check the global dependencies, like engine modules
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
