// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public interface IApplication : IPlugin
	{
		// Start up game/app systems after all plugins have loaded
		bool Start();
		// Execute a single game frame
		bool RunFrame( float delta );
	}
}
