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
			// Register CVars here so they can be tracked and unregistered when the plugin is unloaded
			ConsoleSystem.Commands.ConVarRegistry cvarRegistry = new( assembly, plugin );
			cvarRegistry.RegisterAll();

			if ( !plugin.Init() )
			{
				failedPlugins?.Add( $"{plugin.Name} - failed to initialise (error message: '{plugin.Error}')" );
				cvarRegistry.UnregisterAll();
				return false;
			}

			mConsoleRegistries.Add( plugin, cvarRegistry );

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
		/// Registers a plugin collector.
		/// </summary>
		public static void RegisterPluginCollector( IPluginCollector collector )
		{
			mPluginCollectors.Add( collector );
		}

		/// <summary>
		/// Registers a dependency so that plugins can use the same.
		/// </summary>
		public static void RegisterDependency( string name, Assembly assembly )
		{
			mLoadContext.RegisterDependency( name, assembly );
		}

		/// <returns>All plugins except IApplication-based ones.</returns>
		public static IReadOnlyCollection<IPlugin> GenericPlugins => mGenericPlugins.Values;

		/// <returns>All IApplication plugins and below.</returns>
		public static IReadOnlyCollection<IApplication> Applications => mApplicationPlugins.Values;
	}
}
