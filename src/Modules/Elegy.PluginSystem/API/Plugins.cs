// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;
using System.Reflection;

namespace Elegy.PluginSystem.API
{
	public static partial class Plugins
	{
		public static bool Init( string engineFolder, string[] enginePlugins, bool toolMode )
		{
			mLogger.Log( "Init" );

			mEnginePath = engineFolder;
			mEnginePlugins = enginePlugins;
			mToolMode = toolMode;
			mLoadContext = new();

			RegisterDependency( "Elegy.Common", typeof( IPlugin ).Assembly );
			RegisterDependency( "Elegy.PluginSystem", typeof( Plugins ).Assembly );

			return true;
		}

		public static bool ActivatePlugins()
		{
			mLogger.Log( "ActivatePlugins" );
			mLogger.Log( $"{mPluginLibraries.Count} plugins loaded correctly, initialising them..." );

			List<string> failedPlugins = new();
			mPluginLibraries.ForEach( library =>
			{
				string pluginIdentifier = $"'{library.Metadata.AssemblyName}' at '{library.MetadataPath}'";
				IPlugin? plugin;

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
				mLogSystem.Error( "Plugins failed to load:" );
				for ( int i = 0; i < failedPlugins.Count; i++ )
				{
					mLogSystem.Log( $" * {failedPlugins[i]}" );
				}

				mLogSystem.Log( "Resolve these plugins' errors and try again." );
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
					foreach ( var collector in mPluginCollectors )
					{
						collector.OnPluginUnloaded( app.Value );
					}

					app.Value.Shutdown();
				}
			}

			mApplicationPlugins.Clear();

			foreach ( var plugin in mGenericPlugins )
			{
				if ( plugin.Value.Initialised )
				{
					foreach ( var collector in mPluginCollectors )
					{
						collector.OnPluginUnloaded( plugin.Value );
					}

					plugin.Value.Shutdown();
				}
			}

			mGenericPlugins.Clear();
			mPluginCollectors.Clear();

			UnregisterDependency( "Elegy.Common");
			UnregisterDependency( "Elegy.PluginSystem" );

			mPluginLibraries.Clear();
			try
			{
				mLoadContext?.Unload();
			}
			catch ( Exception ex )
			{
				mLogger.Error( "Woops, looks like unloading ain't allowed" );
				mLogSystem.Log( "OS", $"Message: {ex.Message}" );
			}
		}
	}
}
