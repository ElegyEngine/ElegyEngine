// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Console
	{
		internal static void SetConsole( ConsoleInternal console )
		{
			mConsole = console;
		}

		internal static void Update( float delta )
			=> mConsole.Update( delta );

		private static ConsoleInternal mConsole;
	}
}
