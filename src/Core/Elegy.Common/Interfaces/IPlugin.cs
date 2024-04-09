// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Interfaces
{
	/// <summary>
	/// Generic plugin interface.
	/// </summary>
	public interface IPlugin
	{
		/// <summary>
		/// Called when the plugin is being initialised.
		/// </summary>
		bool Init();

		/// <summary>
		/// Called when the engine and plugins are shutting down.
		/// </summary>
		void Shutdown();

		/// <summary>
		/// The name of this plugin.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// If <see cref="Init"/> returned <c>false</c>, the error message.
		/// </summary>
		string Error { get; }

		/// <summary>
		/// Set by <see cref="Init"/> once initialisation succeeds.
		/// </summary>
		bool Initialised { get; }
	}
}
