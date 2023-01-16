// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Console
	{
		public static void Log( string message = "", ConsoleMessageType type = ConsoleMessageType.Info )
			=> mConsole.Log( $"{message}\n", type );

		public static void LogInline( string message = "", ConsoleMessageType type = ConsoleMessageType.Info )
			=> mConsole.Log( message, type );

		public static void Warning( string message )
			=> Log( $"{message}\n", ConsoleMessageType.Warning );

		public static void Error( string message )
			=> Log( $"{message}\n", ConsoleMessageType.Error );

		public static void Fatal( string message )
			=> Log( $"{message}\n", ConsoleMessageType.Fatal );

		public static void LogArray<T>( params T[] values )
		{
			string arrayString = string.Empty;

			for ( int i = 0; i < values.Length; i++ )
			{
				if ( i != 0 )
				{
					arrayString += ", ";
				}

				arrayString += values[i];
			}

			Log( arrayString );
		}

		/// <summary>
		/// Controls the submission of developer messages.
		/// </summary>
		public static bool Developer { get; set; } = false;

		/// <summary>
		/// Controls the submission of verbose messages.
		/// </summary>
		public static bool Verbose { get; set; } = false;
	}
}
