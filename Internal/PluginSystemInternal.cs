// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy.Internal
{
	internal sealed class PluginSystemInternal
	{
		public PluginSystemInternal()
		{
			Plugins.SetPluginSystem( this );

			mEnginePath = FileSystem.EngineConfig.EngineFolder;
			mEnginePlugins = FileSystem.EngineConfig.EnginePlugins;
		}

		public bool Init()
		{
			Console.Log( "[PluginSystem] Init" );

			// The game etc. will be loaded additionally, when
			// mounted by FileSystem.
			{
				bool someEnginePluginsFailed = false;
				foreach ( string enginePluginPath in mEnginePlugins )
				{
					if ( LoadLibrary( $"{mEnginePath}/{enginePluginPath}" ) == null )
					{
						someEnginePluginsFailed = true;
					}
				}
				if ( someEnginePluginsFailed )
				{
					Console.Warning( "[PluginSystem] One or more engine plugins couldn't load, some things may not work!" );
				}

				bool someGamePluginsFailed = false;
				foreach ( string gamePlugin in FileSystem.CurrentConfig.Plugins )
				{
					if ( LoadLibrary( $"{FileSystem.CurrentGamePath}/{gamePlugin}" ) == null )
					{
						someGamePluginsFailed = true;
					}
				}
				if ( someGamePluginsFailed )
				{
					Console.Warning( "[PluginSystem] One or more base game plugins couldn't load, some things may not work!" );
				}
			}

			List<string> failedPlugins = new();
			mPluginLibraries.ForEach( library =>
			{
				IPlugin? plugin = library.InstantiatePlugin();
				if ( plugin == null )
				{
					failedPlugins.Add( $"'{library.Metadata.AssemblyName}' at '{library.MetadataPath}' - couldn't instantiate" );
					return;
				}

				if ( !plugin.Init() )
				{
					failedPlugins.Add( $"'{library.Metadata.AssemblyName}' at '{library.MetadataPath}' - failed to initialise (error message: '{plugin.Error}')" );
					return;
				}

				if ( plugin is IApplication )
				{
					mApplicationPlugins.Add( library.MetadataPath, plugin as IApplication );
				}
				else
				{
					mGenericPlugins.Add( library.MetadataPath, plugin );
				}
			} );

			if ( failedPlugins.Count > 0 )
			{
				Console.Error( "Plugins failed to load:" );
				for ( int i = 0; i < failedPlugins.Count; i++ )
				{
					Console.Log( $" * {failedPlugins[i]}" );
				}
				Console.Log( "Resolve these plugins' errors and try again." );
				return false;
			}

			failedPlugins.Clear();
			foreach ( var app in mApplicationPlugins )
			{
				if ( !app.Value.Start() )
				{
					failedPlugins.Add( $"'{app.Value.Name}' - failed to start ({app.Value.Error})" );
				}
			}

			if ( failedPlugins.Count > 0 )
			{
				Console.Error( "Applications failed to load:" );
				for ( int i = 0; i < failedPlugins.Count; i++ )
				{
					Console.Log( $" * {failedPlugins[i]}" );
				}
				Console.Log( "Resolve these application errors and try again." );
				return false;
			}

			return true;
		}

		public void Shutdown()
		{
			Console.Log( "[PluginSystem] Shutdown" );

			// First shut down any app/game app
			foreach ( var app in mApplicationPlugins )
			{
				app.Value.Shutdown();
			}
			mApplicationPlugins.Clear();

			foreach ( var plugin in mGenericPlugins )
			{
				plugin.Value.Shutdown();
			}
			mGenericPlugins.Clear();

			mPluginLibraries.Clear();
			try
			{
				mLoadContext.Unload();
			}
			catch ( Exception ex )
			{
				Console.Error( "[PluginSystem] Woops, looks like unloading ain't allowed" );
				Console.Log( $"[OS] Message: {ex.Message}" );
			}
		}

		public IPlugin? GetPlugin( string path )
		{
			return mGenericPlugins.GetValueOrDefault( path );
		}

		public IApplication? GetApplication( string path )
		{
			return mApplicationPlugins.GetValueOrDefault( path );
		}

		public bool UnloadGenericPlugin( IPlugin plugin )
		{
			int i = 0;
			foreach ( var pluginPair in mGenericPlugins )
			{
				if ( pluginPair.Value == plugin )
				{
					mGenericPlugins.Remove( mGenericPlugins.Keys.ElementAt( i ) );
					return true;
				}

				i++;
			}

			return false;
		}

		public bool UnloadApplication( IApplication app )
		{
			int i = 0;
			foreach ( var plugin in mApplicationPlugins )
			{
				if ( plugin.Value == app )
				{
					mApplicationPlugins.Remove( mApplicationPlugins.Keys.ElementAt( i ) );
					return true;
				}

				i++;
			}

			return false;
		}

		private PluginLibrary? LoadLibrary( string path )
		{
			for ( int i = 0; i < mPluginLibraries.Count; i++ )
			{
				if ( mPluginLibraries[i].MetadataPath == path )
				{
					return mPluginLibraries[i];
				}
			}

			Console.Log( $"[PluginSystem] Loading '{path}'..." );

			PluginConfig pluginConfig = new();
			if ( !Text.JsonHelpers.LoadFrom( ref pluginConfig, path ) )
			{
				Console.Error( $"[PluginSystem] Cannot load '{path}'" );
				return null;
			}

			string pluginDirectory = path[..path.LastIndexOf( '/' )];
			string assemblyPath = $"{pluginDirectory}/{pluginConfig.AssemblyName}.dll";

			Assembly assembly;
			try
			{
				assembly = mLoadContext.LoadFromAssemblyPath( $"{Directory.GetCurrentDirectory()}/{assemblyPath}" );
			}
			catch ( Exception ex )
			{
				Console.Error( $"[PluginSystem] Failed to load '{assemblyPath}'" );
				Console.Error( $"[OS] Exception: {ex.Message}" );
				return null;
			}

			PluginLibraryMetadata metadata = new( pluginConfig );
			if ( !metadata.Validate( out var errorMessages ) )
			{
				Console.Error( $"[PluginSystem] '{path}' has invalid data:" );
				foreach ( var error in errorMessages )
				{
					Console.Log( $"[PluginSystem]  * {error}" );
				}
				return null;
			}

			PluginLibrary library = new( assembly, metadata, path );
			if ( !library.LoadedSuccessfully )
			{
				Console.Error( $"[PluginSystem] '{assemblyPath}' implements a non-existing interface '{pluginConfig.ImplementedInterface}'" );
				return null;
			}

			Console.Log( $"[PluginSystem] '{assemblyPath}' loaded successfully" );
			mPluginLibraries.Add( library );
			return library;
		}

		public IReadOnlyCollection<IApplication> ApplicationPlugins => mApplicationPlugins.Values;
		public IReadOnlyCollection<IPlugin> GenericPlugins => mGenericPlugins.Values;

		private Dictionary<string, IApplication> mApplicationPlugins = new();
		private Dictionary<string, IPlugin> mGenericPlugins = new();
		private List<PluginLibrary> mPluginLibraries = new();

		private PluginLoadContext mLoadContext = new();

		private string mEnginePath;
		private string[] mEnginePlugins;
	}
}
