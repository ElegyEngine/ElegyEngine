// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using Elegy.ConsoleCommands;

namespace Elegy
{
	internal sealed class PluginSystemInternal
	{
		public const string Tag = "PluginSystem";

		public PluginSystemInternal()
		{
			Plugins.SetPluginSystem( this );

			mEnginePath = FileSystem.EngineConfig.EngineFolder;
			mEnginePlugins = FileSystem.EngineConfig.EnginePlugins;
		}

		public bool Init()
		{
			Console.Log( Tag, "Init" );

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
					Console.Warning( Tag, "One or more engine plugins couldn't load, some things may not work!" );
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
					Console.Warning( Tag, "One or more base game plugins couldn't load, some things may not work!" );
				}
			}

			Console.Log( Tag, $"{mPluginLibraries.Count} plugins loaded correctly, initialising them..." );

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

				// Register static CVars here so they can be tracked and unregistered when the plugin is unloaded
				ConVarRegistry cvarRegistry = new( library.Assembly );
				cvarRegistry.RegisterAll();

				if ( !plugin.Init() )
				{
					failedPlugins.Add( $"{pluginIdentifier} - failed to initialise (error message: '{plugin.Error}')" );
					cvarRegistry.UnregisterAll();
					return;
				}

				mConsoleRegistries.Add( plugin, cvarRegistry );

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
			Console.Log( Tag, "Shutdown" );

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

			ConsoleCommands.Helpers.HelperManager.UnregisterAllHelpers();

			mPluginLibraries.Clear();
			try
			{
				mLoadContext.Unload();
			}
			catch ( Exception ex )
			{
				Console.Error( Tag, "Woops, looks like unloading ain't allowed" );
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

			Console.Log( Tag, $"Loading '{path}'..." );

			PluginConfig pluginConfig = new();
			if ( !Text.JsonHelpers.LoadFrom( ref pluginConfig, path ) )
			{
				Console.Error( Tag, $"Cannot load '{path}'" );
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
				Console.Error( Tag, $"Failed to load '{assemblyPath}'" );
				Console.Error( "OS", $"Exception: {ex.Message}" );
				return null;
			}

			PluginLibraryMetadata metadata = new( pluginConfig );
			if ( !metadata.Validate( out var errorMessages ) )
			{
				Console.Error( Tag, $"'{path}' has invalid data:" );
				foreach ( var error in errorMessages )
				{
					Console.Log( Tag, " * {error}" );
				}
				return null;
			}

			if ( !metadata.IsCompatible( Engine.MajorVersion, Engine.OldestSupportedMinor ) )
			{
				Console.Error( Tag, $"'{path}' (built for '{metadata.EngineVersionString}') is incompatible (current engine ver. '{Engine.VersionString}')" );
				return null;
			}

			if ( !ConsoleCommands.Helpers.HelperManager.RegisterHelpers( assembly ) )
			{
				Console.Warning( Tag, $"'{assemblyPath}' has one or more console arg. helpers that failed to load, some console commands may not work!" );
			}

			PluginLibrary library = new( assembly, metadata, path );
			if ( !library.LoadedSuccessfully )
			{
				Console.Error( Tag, $"'{assemblyPath}' implements a non-existing interface '{pluginConfig.ImplementedInterface}'" );
				return null;
			}

			Console.Log( Tag, $"'{assemblyPath}' loaded successfully" );
			mPluginLibraries.Add( library );
			return library;
		}

		public IReadOnlyCollection<IApplication> ApplicationPlugins => mApplicationPlugins.Values;
		public IReadOnlyCollection<IPlugin> GenericPlugins => mGenericPlugins.Values;

		private Dictionary<IPlugin, ConVarRegistry> mConsoleRegistries = new();
		private Dictionary<string, IApplication> mApplicationPlugins = new();
		private Dictionary<string, IPlugin> mGenericPlugins = new();
		private List<PluginLibrary> mPluginLibraries = new();

		private PluginLoadContext mLoadContext = new();

		private string mEnginePath;
		private string[] mEnginePlugins;
	}
}
