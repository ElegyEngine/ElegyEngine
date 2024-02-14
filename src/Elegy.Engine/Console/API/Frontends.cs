// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Console
	{
		/// <summary>
		/// Registers a <seealso cref="IConsoleFrontend"/> plugin.
		/// </summary>
		public static bool AddFrontend( IConsoleFrontend frontend )
			=> mConsole.AddFrontend( frontend );

		/// <summary>
		/// Unregisters a <seealso cref="IConsoleFrontend"/>.
		/// </summary>
		public static bool RemoveFrontend( IConsoleFrontend frontend )
			=> mConsole.RemoveFrontend( frontend );
	}
}
