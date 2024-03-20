// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces;

namespace Elegy.Engine.API
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
			=> mPluginSystem.LoadPlugin( path );

		/// <summary>
		/// Obtains a plugin by its path, e.g. <c>engine/plugins/RenderStandard</c>
		/// </summary>
		public static IPlugin? GetPlugin( string path )
			=> mPluginSystem.GetPlugin( path );

		/// <summary>
		/// Same as GetPlugin, but for application plugins.
		/// </summary>
		public static IApplication? GetApplication( string path )
			=> mPluginSystem.GetApplication( path );

		/// <summary>
		/// Registers a plugin.
		/// </summary>
		public static bool RegisterPlugin( IPlugin plugin )
			=> mPluginSystem.RegisterPlugin( plugin );

		/// <returns>All plugins except IApplication-based ones.</returns>
		public static IReadOnlyCollection<IPlugin> GenericPlugins => mPluginSystem.GenericPlugins;

		/// <returns>All IApplication plugins and below.</returns>
		public static IReadOnlyCollection<IApplication> Applications => mPluginSystem.ApplicationPlugins;
	}
}
