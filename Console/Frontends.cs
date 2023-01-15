// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Console
	{
		public static bool AddFrontend( IConsoleFrontend frontend )
			=> mConsole.AddFrontend( frontend );
	}
}
