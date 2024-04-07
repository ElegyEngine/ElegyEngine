// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.API
{
	public static partial class Console
	{
		internal static void SetConsole( ConsoleInternal? console )
			=> mConsole = console;
		

		internal static void Update( float delta )
			=> mConsole.Update( delta );

		private static ConsoleInternal? mConsole;
	}
}
