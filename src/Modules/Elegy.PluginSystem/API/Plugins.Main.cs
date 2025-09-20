// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;
using System.Reflection;

namespace Elegy.PluginSystem.API
{
	/// <summary>
	/// Elegy plugin system interface.
	/// </summary>
	public static partial class Plugins
	{
		/// <summary>
		/// Reads plugin metadata, and the plugin later gets instantiated in <see cref="ActivatePlugins"/>.
		/// Calling this after plugins have been activated has no effect.
		/// </summary>
		public static bool ScanLibrary( string path )
			=> LoadLibrary( path ) is not null;

		/// <summary>
		/// Reads plugin metadata, loads a plugin assembly and returns an instance of the plugin.
		/// </summary>
		/// <param name="path">Directory where the plugin is located.
		/// "game/plugins/something" will load "game/plugins/something/plugin.json"
		/// and load the assembly based on that plugin configuration.</param>
		/// <returns>null if the path is invalid or if it's missing needed files,
		/// otherwise returns a valid plugin instance.</returns>
		public static IPlugin? LoadPlugin( string path )
		{
			IPlugin? plugin = GetPlugin( path );
			if ( plugin is not null )
			{
				return plugin;
			}

			PluginLibrary? library = LoadLibrary( $"{path}/pluginConfig.json" );
			if ( library?.Factory is null )
			{
				return null;
			}

			plugin = library.Factory();
			if ( plugin is null )
			{
				return null;
			}

			if ( !RegisterPlugin( plugin, library.Assembly, path ) )
			{
				mLogger.Warning( $"LoadPlugin: failed to load plugin '{path}'" );
				mLogger.Warning( $"Reason: {plugin.Error}" );
				return null;
			}

			return plugin;
		}

		/// <summary>
		/// Obtains a plugin by its path, e.g. <c>engine/plugins/RenderStandard</c>
		/// </summary>
		public static IPlugin? GetPlugin( string path )
		{
			return mGenericPlugins.GetValueOrDefault( path );
		}

		/// <summary>
		/// Unloads a plugin.
		/// </summary>
		public static bool UnloadPlugin( IPlugin plugin )
		{
			int i = 0;
			foreach ( var pluginPair in mGenericPlugins )
			{
				if ( pluginPair.Value == plugin )
				{
					foreach ( var collector in mPluginCollectors )
					{
						collector.OnPluginUnloaded( plugin );
					}

					mGenericPlugins.Remove( mGenericPlugins.Keys.ElementAt( i ) );
					return true;
				}

				i++;
			}

			return false;
		}

		/// <summary>
		/// Same as GetPlugin, but for application plugins.
		/// </summary>
		public static IApplication? GetApplication( string path )
		{
			return mApplicationPlugins.GetValueOrDefault( path );
		}

		/// <summary>
		/// Same as <see cref="UnloadPlugin(IPlugin)"/> but for application plugins.
		/// </summary>
		public static bool UnloadApplication( IApplication app )
		{
			int i = 0;
			foreach ( var plugin in mApplicationPlugins )
			{
				if ( plugin.Value == app )
				{
					foreach ( var collector in mPluginCollectors )
					{
						collector.OnPluginUnloaded( app );
					}

					mApplicationPlugins.Remove( mApplicationPlugins.Keys.ElementAt( i ) );
					return true;
				}

				i++;
			}

			return false;
		}

		/// <summary>
		/// Registers a plugin.
		/// </summary>
		public static bool RegisterPlugin( IPlugin plugin, Assembly? assembly = null, string? metadataPath = null, List<string>? failedPlugins = null )
		{
			foreach ( var collector in mPluginCollectors )
			{
				collector.BeforePluginLoaded( assembly, plugin );
			}

			if ( !plugin.Init() )
			{
				failedPlugins?.Add( $"{plugin.Name} - failed to initialise (error message: '{plugin.Error}')" );
				foreach ( var collector in mPluginCollectors )
				{
					collector.OnPluginFailed( assembly, plugin );
				}
				return false;
			}

			if ( plugin is IApplication applicationPlugin )
			{
				mApplicationPlugins.Add( metadataPath ?? plugin.Name, applicationPlugin );
			}
			else
			{
				mGenericPlugins.Add( metadataPath ?? plugin.Name, plugin );
			}

			foreach ( var collector in mPluginCollectors )
			{
				collector.OnPluginLoaded( plugin );
			}

			return true;
		}

		/// <summary>
		/// Instantiates and registers a plugin collector of type <typeparamref name="T"/>.
		/// </summary>
		public static void RegisterPluginCollector<T>() where T : IPluginCollector, new()
			=> RegisterPluginCollector( new T() );

		/// <summary>
		/// Registers a plugin collector.
		/// </summary>
		public static void RegisterPluginCollector( IPluginCollector collector )
		{
			if ( mPluginCollectors.Contains( collector ) )
			{
				mLogger.Warning( $"Tried adding an already registered collector '{collector.GetType().Name}'" );
				return;
			}

			mPluginCollectors.Add( collector );
		}

		/// <summary>
		/// Unregisters a plugin collector.
		/// </summary>
		public static bool UnregisterPluginCollector<TCollector>() where TCollector : IPluginCollector
		{
			for ( int i = 0; i < mPluginCollectors.Count; i++ )
			{
				if ( mPluginCollectors[i].GetType() == typeof( TCollector ) )
				{
					mPluginCollectors.RemoveAt( i );
					return true;
				}
			}

			mLogger.Warning( $"Tried removing a non-registered collector '{typeof( TCollector ).Name}'" );
			return false;
		}

		/// <summary>
		/// Registers a dependency so that plugins can use the same.
		/// </summary>
		public static void RegisterDependency( string name, Assembly assembly )
		{
			if ( mLoadContext is null )
			{
				mLogger.Error( $"Tried registering a dependency '{name}' while the plugin system is not initialised" );
				return;
			}
			
			mLoadContext.RegisterDependency( name, assembly );
		}
		
		/// <summary>
		/// Registers a dependency so that plugins can use the same.
		/// </summary>
		public static void RegisterDependency<T>( string name )
			=> RegisterDependency( name, typeof( T ).Assembly );

		/// <summary>
		/// Unregisters a dependency.
		/// </summary>
		public static bool UnregisterDependency( string name )
		{
			if ( mLoadContext is null )
			{
				mLogger.Error( $"Tried registering a dependency '{name}' while the plugin system is not initialised" );
				return false;
			}
			
			return mLoadContext.UnregisterDependency( name );
		}

		/// <returns>All plugins except IApplication-based ones.</returns>
		public static IReadOnlyCollection<IPlugin> GenericPlugins => mGenericPlugins.Values;

		/// <returns>All IApplication plugins and below.</returns>
		public static IReadOnlyCollection<IApplication> Applications => mApplicationPlugins.Values;
	}
}
