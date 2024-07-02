// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;
using System.Reflection;

using Console = Elegy.ConsoleSystem.API.Console;

namespace Elegy.PluginSystem.API
{
	public static partial class Plugins
	{
		private static TaggedLogger mLogger = new( "PluginSystem" );

		private static Dictionary<IPlugin, ConsoleSystem.Commands.ConVarRegistry> mConsoleRegistries = new();
		private static Dictionary<string, IApplication> mApplicationPlugins = new();
		private static Dictionary<string, IPlugin> mGenericPlugins = new();
		private static List<PluginLibrary> mPluginLibraries = new();
		private static List<IPluginCollector> mPluginCollectors = new();

		private static PluginLoadContext? mLoadContext = null;

		private static string EngineVersionString => $"v{mEngineMajorVersion}.{mEngineMinorVersion}";
		private static int mEngineMajorVersion;
		private static int mEngineMinorVersion;
		private static string mEnginePath;
		private static string[] mEnginePlugins;
		private static bool mToolMode;

		private static PluginLibrary? LoadLibrary( string path )
		{
			for ( int i = 0; i < mPluginLibraries.Count; i++ )
			{
				if ( mPluginLibraries[i].MetadataPath == path )
				{
					return mPluginLibraries[i];
				}
			}

			mLogger.Log( $"Loading '{path}'..." );

			string? fullPath = Files.PathTo( path, PathFlags.File );
			if ( fullPath is null )
			{
				mLogger.Error( $"Cannot load '{path}', it doesn't exist" );
				return null;
			}

			PluginConfig pluginConfig = new();
			if ( !Common.Text.JsonHelpers.LoadFrom( ref pluginConfig, fullPath ) )
			{
				mLogger.Error( $"Cannot load '{path}'" );
				return null;
			}

			string pluginDirectory = fullPath[..fullPath.LastIndexOf( '/' )];
			string fullPluginDirectory = $"{Directory.GetCurrentDirectory()}/{pluginDirectory}";
			string assemblyPath = $"{pluginDirectory}/{pluginConfig.AssemblyName}.dll";
			string fullAssemblyPath = $"{Directory.GetCurrentDirectory()}/{assemblyPath}";

			mLoadContext.AddPluginSearchPaths( path, fullPluginDirectory );

			Assembly assembly;
			try
			{
				assembly = mLoadContext.LoadFromAssemblyPath( fullAssemblyPath );
			}
			catch ( Exception ex )
			{
				mLogger.Error( $"Failed to load '{assemblyPath}'" );
				Console.Error( "OS", $"Exception: {ex.Message}" );
				return null;
			}

			PluginLibraryMetadata metadata = new( pluginConfig );
			if ( !metadata.Validate( out var errorMessages ) )
			{
				mLogger.Error( $"'{path}' has invalid data:" );
				foreach ( var error in errorMessages )
				{
					mLogger.Log( $" * {error}" );
				}
				return null;
			}

			if ( !metadata.IsCompatible( mEngineMajorVersion, mEngineMinorVersion ) )
			{
				mLogger.Error( $"'{path}' (built for '{metadata.EngineVersionString}') is incompatible (current engine ver. '{EngineVersionString}')" );
				return null;
			}

			if ( !ConsoleSystem.Commands.HelperManager.RegisterHelpers( assembly ) )
			{
				mLogger.Warning( $"'{assemblyPath}' has one or more console arg. helpers that failed to load, some console commands may not work!" );
			}

			PluginLibrary library = new( assembly, metadata, path );
			if ( !library.LoadedSuccessfully )
			{
				mLogger.Error( $"'{assemblyPath}' implements a non-existing interface '{pluginConfig.ImplementedInterface}'" );
				return null;
			}

			mLogger.Log( $"'{assemblyPath}' loaded successfully" );
			mPluginLibraries.Add( library );

			return library;
		}
	}
}
