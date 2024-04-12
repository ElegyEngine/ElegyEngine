// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ConsoleSystem
{
	/// <summary>
	/// Scope-based tagged logger.
	/// </summary>
	public struct TaggedLogger
	{
		/// <summary>
		/// Constructor for simple tags.
		/// </summary>
		public TaggedLogger( string tag )
		{
			Tag = tag;
		}

		/// <summary>
		/// Constructor for nested tags.
		/// </summary>
		public TaggedLogger( in TaggedLogger parent, string tag )
		{
			Tag = $"{parent.Tag}.{tag}";
		}

		/// <summary>
		/// Tag to use when logging.
		/// </summary>
		public readonly string Tag { get; }

		/// <summary>
		/// See <see cref="Console.Log(string, ConsoleMessageType)"/>.
		/// </summary>
		public void Log( string message )
			=> Console.Log( Tag, message );

		/// <summary>
		/// See <see cref="Log(string)"/>.
		/// </summary>
		public void LogIf( bool condition, string message )
		{
			if ( condition )
			{
				Log( message );
			}
		}

		/// <summary>
		/// See <see cref="Console.Warning(string)"/>.
		/// </summary>
		public void Warning( string message )
			=> Console.Warning( Tag, message );

		/// <summary>
		/// See <see cref="Warning(string)"/>.
		/// </summary>
		public void WarningIf( bool condition, string message )
		{
			if ( condition )
			{
				Warning( message );
			}
		}

		/// <summary>
		/// See <see cref="Console.Error(string)"/>.
		/// </summary>
		public void Error( string message )
			=> Console.Error( Tag, message );

		/// <summary>
		/// See <see cref="Console.Fatal(string)"/>.
		/// </summary>
		public void Fatal( string message )
			=> Console.Fatal( Tag, message );

		/// <summary>
		/// See <see cref="ConsoleMessageType.Developer"/>.
		/// </summary>
		public void Developer( string message )
			=> Console.Log( Tag, message, ConsoleMessageType.Developer );

		/// <summary>
		/// See <see cref="ConsoleMessageType.Verbose"/>.
		/// </summary>
		public void Verbose( string message )
			=> Console.Log( Tag, message, ConsoleMessageType.Verbose );

		/// <summary>
		/// See <see cref="Console.Success(string)"/>.
		/// </summary>
		public void Success( string message )
			=> Console.Success( Tag, message );
	}
}
