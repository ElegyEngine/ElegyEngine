// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Console
	{
		#region Standard logging

		public static void Log( string message = "", ConsoleMessageType type = ConsoleMessageType.Info )
			=> mConsole.Log( $"{message}\n", type );

		public static void LogInline( string message = ""  )
			=> mConsole.Log( message, ConsoleMessageType.Info );

		public static void Newline( ConsoleMessageType type = ConsoleMessageType.Info )
			=> mConsole.Log( "\n", type );

		public static void Success( string message )
			=> Log( $"{Green}{message}", ConsoleMessageType.Success );

		public static void Warning( string message )
			=> Log( $"{message}", ConsoleMessageType.Warning );

		public static void Error( string message )
			=> Log( $"{message}", ConsoleMessageType.Error );

		public static void Fatal( string message )
			=> Log( $"{message}", ConsoleMessageType.Fatal );

		#endregion

		#region Tagged logging

		// Logging methods with the addition of a system tag
		// E.g. Log( "Engine", "Starting up..." ) becomes "[Engine] Starting up"

		public static void Log( string tag, string message, ConsoleMessageType type = ConsoleMessageType.Info )
			=> Log( $"{Yellow}[{tag}]{White} {message}", type );

		public static void Success( string tag, string message )
			=> Log( $"{Yellow}[{tag}]{Green} {message}", ConsoleMessageType.Success );

		public static void Warning( string tag, string message )
			=> Warning( $"{Yellow}[{tag}]{Yellow} {message}" );

		public static void Error( string tag, string message )
			=> Error( $"{Yellow}[{tag}]{Red} {message}" );

		public static void Fatal( string tag, string message )
			=> Fatal( $"{Yellow}[{tag}]{Red} {message}" );

		#endregion

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
