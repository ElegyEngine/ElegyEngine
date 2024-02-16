// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleCommands;

namespace Elegy
{
	public static partial class Console
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns><c>true</c> upon success, <c>false</c> upon encountering
		/// a duplicate or other error.</returns>
		public static bool RegisterCommand( ConsoleCommand command )
			=> mConsole.RegisterCommand( command );

		/// <summary>
		/// 
		/// </summary>
		public static void UnregisterCommand( ConsoleCommand command )
			=> mConsole.UnregisterCommand( command );

		/// <summary>
		/// 
		/// </summary>
		public static bool Execute( string command )
			=> mConsole.Execute( command );

		/// <summary>
		/// Commandline arguments passed to the launcher.
		/// </summary>
		public static StringDictionary Arguments
			=> mConsole.GetArguments();
	}
}
