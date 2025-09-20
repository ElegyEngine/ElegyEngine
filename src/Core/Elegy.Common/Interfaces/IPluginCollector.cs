// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Reflection;

namespace Elegy.Common.Interfaces
{
	/// <summary>
	/// Handles the loading and unloading of plugins.
	/// Used by other Elegy systems to "catch" their respective
	/// plugin types and similar things.
	/// </summary>
	public interface IPluginCollector
	{
		/// <summary>
		/// What happens when the plugin's assembly has been loaded.
		/// </summary>
		void OnAssemblyLoaded( Assembly assembly ) {}

		/// <summary>
		/// What happens when the plugin is about to be loaded.
		/// </summary>
		void BeforePluginLoaded( Assembly? assembly, IPlugin plugin ) {}

		/// <summary>
		/// What happens when the plugin has been loaded.
		/// </summary>
		void OnPluginLoaded( IPlugin plugin ) {}

		/// <summary>
		/// What happens when the plugin has been unloaded.
		/// </summary>
		void OnPluginUnloaded( IPlugin plugin ) {}

		/// <summary>
		/// What happens when a plugin has failed to load.
		/// </summary>
		void OnPluginFailed( Assembly? assembly, IPlugin plugin ) {}
	}
}
