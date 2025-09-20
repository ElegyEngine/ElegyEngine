// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.Common.Interfaces.Services;
using Elegy.Common.Utilities;
using System.Reflection;

namespace Elegy.PluginSystem.API
{
	public static partial class Plugins
	{
		private static TaggedLogger mLogger = new( "PluginSystem" );
		private static ILogSystem mLogSystem = ElegyInterfaceLocator.GetLogSystem();
		private static IFileSystem mFileSystem = ElegyInterfaceLocator.GetFileSystem();

		private static Dictionary<string, IApplication> mApplicationPlugins = new();
		private static Dictionary<string, IPlugin> mGenericPlugins = new();
		private static List<PluginLibrary> mPluginLibraries = new();
		private static List<IPluginCollector> mPluginCollectors = new();

		private static PluginLoadContext? mLoadContext;

		private static string EngineVersionString => $"v{mEngineMajorVersion}.{mEngineMinorVersion}";
		private static int mEngineMajorVersion;
		private static int mEngineMinorVersion;
		private static string mEnginePath = string.Empty;
		private static string[] mEnginePlugins = [];
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

			string? fullPath = mFileSystem.PathToFile( path );
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

			Debug.Assert( mLoadContext is not null );
			mLoadContext.AddPluginSearchPaths( path, fullPluginDirectory );

			Assembly assembly;
			try
			{
				assembly = mLoadContext.LoadFromAssemblyPath( fullAssemblyPath );
			}
			catch ( Exception ex )
			{
				mLogger.Error( $"Failed to load '{assemblyPath}'" );
				mLogSystem.Error( "OS", $"Exception: {ex.Message}" );
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

			PluginLibrary library = new( assembly, metadata, path );
			if ( !library.LoadedSuccessfully )
			{
				mLogger.Error( $"'{assemblyPath}' implements a non-existing interface '{pluginConfig.ImplementedInterface}'" );
				return null;
			}

			foreach ( var collector in mPluginCollectors )
			{
				collector.OnAssemblyLoaded( assembly );
			}

			mLogger.Log( $"'{assemblyPath}' loaded successfully" );
			mPluginLibraries.Add( library );

			return library;
		}
	}
}
