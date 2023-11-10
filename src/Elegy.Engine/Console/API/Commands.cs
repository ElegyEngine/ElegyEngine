// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Console
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns><c>true</c> upon success, <c>false</c> upon encountering
		/// a duplicate or other error.</returns>
		public static bool RegisterCommand( ConCommand command )
			=> mConsole.RegisterCommand( command );

		/// <summary>
		/// 
		/// </summary>
		public static void UnregisterCommand( ConCommand command )
			=> mConsole.UnregisterCommand( command );

		/// <summary>
		/// 
		/// </summary>
		public static bool Execute( string command )
			=> mConsole.Execute( command );
	}
}
