// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ConsoleSystem.API
{
	/// <summary>
	/// Elegy console interface.
	/// </summary>
	public static partial class Console
	{
		#region Standard logging

		/// <summary>
		/// Logs a message into the console, with a newline at the end.
		/// </summary>
		public static void Log( string message = "", ConsoleMessageType type = ConsoleMessageType.Info )
			=> LogInternal( $"{message}\n", type );

		/// <summary>
		/// Logs a message into the console without a newline.
		/// </summary>
		public static void LogInline( string message = ""  )
			=> LogInternal( message, ConsoleMessageType.Info );

		/// <summary>
		/// Inserts a newline into the console.
		/// </summary>
		public static void Newline( ConsoleMessageType type = ConsoleMessageType.Info )
			=> LogInternal( "\n", type );

		/// <summary>
		/// Prints a success message.
		/// </summary>
		public static void Success( string message )
			=> Log( $"{Green}{message}", ConsoleMessageType.Success );

		/// <summary>
		/// Prints a warning message.
		/// </summary>
		public static void Warning( string message )
			=> Log( $"{message}", ConsoleMessageType.Warning );

		/// <summary>
		/// Prints an error message.
		/// </summary>
		public static void Error( string message )
			=> Log( $"{message}", ConsoleMessageType.Error );

		/// <summary>
		/// Prints a fatal error message.
		/// </summary>
		public static void Fatal( string message )
			=> Log( $"{message}", ConsoleMessageType.Fatal );

		#endregion

		#region Tagged logging

		// Logging methods with the addition of a system tag
		// E.g. Log( "Engine", "Starting up..." ) becomes "[Engine] Starting up"

		/// <summary>
		/// Prints a message with a tag. E.g. '[Game] Map loaded!'
		/// </summary>
		public static void Log( string tag, string message, ConsoleMessageType type = ConsoleMessageType.Info )
			=> Log( $"{Yellow}[{tag}]{White} {message}", type );

		/// <summary>
		/// Prints a success message with a tag. E.g. '[Game] Map loaded!'
		/// </summary>
		public static void Success( string tag, string message )
			=> Log( $"{Yellow}[{tag}]{Green} {message}", ConsoleMessageType.Success );

		/// <summary>
		/// Prints a warning with a tag. E.g. '[ModelManager] Model has too many vertices!'
		/// </summary>
		public static void Warning( string tag, string message )
			=> Warning( $"{Yellow}[{tag}]{Yellow} {message}" );

		/// <summary>
		/// Prints an error with a tag. E.g. '[ModelManager] Can't find asset!'
		/// </summary>
		public static void Error( string tag, string message )
			=> Error( $"{Yellow}[{tag}]{Red} {message}" );

		/// <summary>
		/// Prints a fatal error with a tag. E.g. '[Engine] OS could not allocate memory'
		/// </summary>
		public static void Fatal( string tag, string message )
			=> Fatal( $"{Yellow}[{tag}]{Red} {message}" );

		#endregion

		/// <summary>
		/// Prints an array of values in a single line.
		/// </summary>
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
