// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.FileSystem.API;
using System.Reflection;

using Console = Elegy.ConsoleSystem.API.Console;

namespace Elegy.PluginSystem.API
{
	public static partial class Plugins
	{
		public static bool Init( LaunchConfig config )
		{
			mLogger.Log( "Init" );

			mEnginePath = config.Engine.EngineFolder;
			mEnginePlugins = config.Engine.EnginePlugins;

			mLoadContext = new();

			return true;
		}

		public static bool PostInit()
		{
			mLogger.Log( "PostInit" );

			// The game etc. will be loaded additionally, when
			// mounted by the file system.
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
				foreach ( string gamePlugin in Files.CurrentConfig.Plugins )
				{
					if ( LoadLibrary( $"{Files.CurrentGamePath}/{gamePlugin}/pluginConfig.json" ) == null )
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

		public static void Shutdown()
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
			mPluginCollectors.Clear();
			mConsoleRegistries.Clear();

			ConsoleSystem.Commands.HelperManager.UnregisterAllHelpers();

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
	}
}
