// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Application plugin interface.
	/// This is implemented by games and tools.
	/// </summary>
	public interface IApplication : IPlugin
	{
		/// <summary>
		/// Start up game/app systems after all plugins have loaded.
		/// </summary>
		/// <returns></returns>
		bool Start();

		/// <summary>
		/// Execute a single game frame.
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		bool RunFrame( float delta );

		/// <summary>
		/// Execute a single physics frame.
		/// </summary>
		/// <param name="delta"></param>
		void RunPhysicsFrame( float delta );

		/// <summary>
		/// Handle user input.
		/// </summary>
		/// <param name="event"></param>
		void HandleInput( InputEvent @event );
	}
}
