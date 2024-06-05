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
		private TaggedLogger mLogger = new( "DllLoader" );
		private List<(string, Assembly)> mDependencies = new();
		private Dictionary<string, List<string>> mSearchPaths = new();

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

		public void AddPluginSearchPaths( string plugin, string path )
		{
			var paths = mSearchPaths.GetOrAdd( plugin, [] );
			if ( paths.Contains( path ) )
			{
				mLogger.Warning( $"Plugin '{plugin}' already has '{path}' in its search paths" );
				return;
			}

			paths.Add( path );
		}

		public bool ClearPluginSearchPaths( string plugin )
		{
			if ( !mSearchPaths.ContainsKey( plugin ) )
			{
				mLogger.Warning( $"Plugin '{plugin}' is not in search paths" );
				return false;
			}

			mSearchPaths[plugin].Clear();
			mSearchPaths.Remove( plugin );

			return true;
		}

		private Assembly? TryLoadingAssemblyInDirectory( string path, string assemblyName )
		{
			var dllFiles = Directory.EnumerateFiles( path, "*.dll", SearchOption.AllDirectories );
			foreach ( var file in dllFiles )
			{
				var fileNoExtension = Path.ChangeExtension( file, null );
				if ( fileNoExtension.EndsWith( assemblyName ) )
				{
					// Will this cause infinite recursion? I fear it might call Load
					Assembly assembly = LoadFromAssemblyPath( file );
					RegisterDependency( fileNoExtension, assembly );

					return assembly;
				}
			}

			return null;
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

			// Check shared plugin dependencies
			// TODO: make this rigorous, heavy testing required
			// This would be best done in a special Elegy test app using the
			// barebones console app setup (like MapCompiler) with some plugins
			// that have messed-up dependencies for testing
			foreach ( var plugin in mSearchPaths )
			{
				foreach ( var searchPath in plugin.Value )
				{
					Assembly? assembly = TryLoadingAssemblyInDirectory( searchPath, assemblyName.Name );
					if ( assembly is not null )
					{
						return assembly;
					}
				}
			}

			// Fallback to the host context
			return null;
		}
	}
}
