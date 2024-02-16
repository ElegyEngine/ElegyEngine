// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy
{
	internal sealed class PluginSystemInternal
	{
		private TaggedLogger mLogger = new( "PluginSystem" );

		public PluginSystemInternal()
		{
			Plugins.SetPluginSystem( this );

			mEnginePath = FileSystem.EngineConfig.EngineFolder;
			mEnginePlugins = FileSystem.EngineConfig.EnginePlugins;
		}

		public bool Init()
		{
			mLogger.Log( "Init" );

			// The game etc. will be loaded additionally, when
			// mounted by FileSystem.
			{
				bool someEnginePluginsFailed = false;
				foreach ( string enginePluginPath in mEnginePlugins )
				{
					if ( LoadLibrary( $"{mEnginePath}/{enginePluginPath}/pluginConfig.json" ) == null )
					{
						someEnginePluginsFailed = true;
					}
				}
				if ( someEnginePluginsFailed )
				{
					mLogger.Warning( "One or more engine plugins couldn't load, some things may not work!" );
				}

				bool someGamePluginsFailed = false;
				foreach ( string gamePlugin in FileSystem.CurrentConfig.Plugins )
				{
					if ( LoadLibrary( $"{FileSystem.CurrentGamePath}/{gamePlugin}/pluginConfig.json" ) == null )
					{
						someGamePluginsFailed = true;
					}
				}
				if ( someGamePluginsFailed )
				{
					mLogger.Warning( "One or more base game plugins couldn't load, some things may not work!" );
				}
			}

			mLogger.Log( $"{mPluginLibraries.Count} plugins loaded correctly, initialising them..." );

			List<string> failedPlugins = new();
			mPluginLibraries.ForEach( library =>
			{
				string pluginIdentifier = $"'{library.Metadata.AssemblyName}' at '{library.MetadataPath}'";
				IPlugin? plugin = null;

				try
				{
					plugin = library.InstantiatePlugin();
				}
				catch ( TargetInvocationException ex )
				{
					failedPlugins.Add( $"{pluginIdentifier} - exception in the plugin's constructor:" );
					failedPlugins.Add( $"    +---> '{ex.InnerException?.Message ?? "null"}'" );
					return;
				}
				catch ( Exception ex )
				{
					failedPlugins.Add( $"{pluginIdentifier} - exception: '{ex.Message}'" );
					return;
				}

				if ( plugin == null )
				{
					failedPlugins.Add( $"{pluginIdentifier} - couldn't allocate" );
					return;
				}

				RegisterPlugin( plugin, library.Assembly, library.MetadataPath, failedPlugins );
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
			mLogger.Log( "Shutdown" );

			// First shut down any app/game app
			foreach ( var app in mApplicationPlugins )
			{
				if ( app.Value.Initialised )
				{
					mConsoleRegistries[app.Value].UnregisterAll();
					app.Value.Shutdown();
				}
			}
			mApplicationPlugins.Clear();

			foreach ( var plugin in mGenericPlugins )
			{
				if ( plugin.Value.Initialised )
				{
					mConsoleRegistries[plugin.Value].UnregisterAll();
					plugin.Value.Shutdown();
				}
			}
			mGenericPlugins.Clear();
			mConsoleRegistries.Clear();

			ConsoleCommands.HelperManager.UnregisterAllHelpers();

			mPluginLibraries.Clear();
			try
			{
				mLoadContext.Unload();
			}
			catch ( Exception ex )
			{
				mLogger.Error( "Woops, looks like unloading ain't allowed" );
				Console.Log( "OS", $"Message: {ex.Message}" );
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

		public bool RegisterPlugin( IPlugin plugin, Assembly? assembly = null, string? metadataPath = null, List<string>? failedPlugins = null )
		{
			// Register CVars here so they can be tracked and unregistered when the plugin is unloaded
			ConsoleCommands.ConVarRegistry cvarRegistry = new( assembly, plugin );
			cvarRegistry.RegisterAll();

			if ( !plugin.Init() )
			{
				failedPlugins?.Add( $"{plugin.Name} - failed to initialise (error message: '{plugin.Error}')" );
				cvarRegistry.UnregisterAll();
				return false;
			}

			mConsoleRegistries.Add( plugin, cvarRegistry );

			if ( plugin is IApplication )
			{
				mApplicationPlugins.Add( metadataPath ?? plugin.Name, plugin as IApplication );
			}
			else
			{
				mGenericPlugins.Add( metadataPath ?? plugin.Name, plugin );
			}

			return true;
		}

		public IPlugin? LoadPlugin( string path )
		{
			IPlugin? plugin = GetPlugin( path );
			if ( plugin is not null )
			{
				return plugin;
			}

			PluginLibrary? library = LoadLibrary( $"{path}/pluginConfig.json" );
			if ( library is null )
			{
				return null;
			}

			plugin = library.Factory();
			if ( !RegisterPlugin( plugin, library.Assembly, path ) )
			{
				mLogger.Warning( $"LoadPlugin: failed to load plugin '{path}'" );
				mLogger.Warning( $"Reason: {plugin.Error}" );
				return null;
			}

			return plugin;
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

			mLogger.Log( $"Loading '{path}'..." );

			string? fullPath = FileSystem.PathTo( path, PathFlags.File );
			if ( fullPath is null )
			{
				mLogger.Error( $"Cannot load '{path}', it doesn't exist" );
				return null;
			}

			PluginConfig pluginConfig = new();
			if ( !Text.JsonHelpers.LoadFrom( ref pluginConfig, fullPath ) )
			{
				mLogger.Error( $"Cannot load '{path}'" );
				return null;
			}

			string pluginDirectory = fullPath[..fullPath.LastIndexOf( '/' )];
			string assemblyPath = $"{pluginDirectory}/{pluginConfig.AssemblyName}.dll";

			Assembly assembly;
			try
			{
				assembly = mLoadContext.LoadFromAssemblyPath( $"{Directory.GetCurrentDirectory()}/{assemblyPath}" );
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

			if ( !metadata.IsCompatible( Engine.MajorVersion, Engine.OldestSupportedMinor ) )
			{
				mLogger.Error( $"'{path}' (built for '{metadata.EngineVersionString}') is incompatible (current engine ver. '{Engine.VersionString}')" );
				return null;
			}

			if ( !ConsoleCommands.HelperManager.RegisterHelpers( assembly ) )
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

		public IReadOnlyCollection<IApplication> ApplicationPlugins => mApplicationPlugins.Values;
		public IReadOnlyCollection<IPlugin> GenericPlugins => mGenericPlugins.Values;

		private Dictionary<IPlugin, ConsoleCommands.ConVarRegistry> mConsoleRegistries = new();
		private Dictionary<string, IApplication> mApplicationPlugins = new();
		private Dictionary<string, IPlugin> mGenericPlugins = new();
		private List<PluginLibrary> mPluginLibraries = new();

		private PluginLoadContext mLoadContext = new();

		private string mEnginePath;
		private string[] mEnginePlugins;
	}
}
