// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;

namespace Elegy.Common.Interfaces
{
	/// <summary>
	/// Application plugin interface.
	/// This is implemented by games and tools.
	/// </summary>
	public interface IApplication : IPlugin
	{
		/// <summary>
		/// Start up game/app systems after all plugins and systems have loaded.
		/// </summary>
		/// <returns></returns>
		bool Start();

		/// <summary>
		/// Execute a single game frame.
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		bool RunFrame( float delta );
	}
}
