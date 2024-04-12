// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

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
		/// What happens when the plugin is loaded.
		/// </summary>
		void OnPluginLoaded( IPlugin plugin );

		/// <summary>
		/// What happens when the plugin is unloaded.
		/// </summary>
		void OnPluginUnloaded( IPlugin plugin );
	}
}
