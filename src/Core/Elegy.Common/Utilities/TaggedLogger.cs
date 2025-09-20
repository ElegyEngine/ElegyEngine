// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces.Services;

namespace Elegy.Common.Utilities
{
	/// <summary>
	/// Scope-based tagged logger.
	/// </summary>
	public struct TaggedLogger
	{
		private ILogSystem LogSystem => ElegyInterfaceLocator.GetLogSystem();

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
		public string Tag { get; }

		/// <summary>
		/// Prints a message.
		/// </summary>
		public void Log( string message )
			=> LogSystem.Log( Tag, message );

		/// <summary>
		/// Conditionally prints a message.
		/// </summary>
		public void LogIf( bool condition, string message )
		{
			if ( condition )
			{
				Log( message );
			}
		}

		/// <summary>
		/// Prints a warning message.
		/// </summary>
		public void Warning( string message )
			=> LogSystem.Warning( Tag, message );

		/// <summary>
		/// Conditionally prints a warning message.
		/// </summary>
		public void WarningIf( bool condition, string message )
		{
			if ( condition )
			{
				Warning( message );
			}
		}

		/// <summary>
		/// Prints an error message.
		/// </summary>
		public void Error( string message )
			=> LogSystem.Error( Tag, message );

		/// <summary>
		/// Conditionally prints an error message.
		/// </summary>
		public void ErrorIf( bool condition, string message )
		{
			if ( condition )
			{
				Error( message );
			}
		}

		/// <summary>
		/// Prints a fatal error message.
		/// </summary>
		public void Fatal( string message )
			=> LogSystem.Fatal( Tag, message );

		/// <summary>
		/// Prints a developer-only message.
		/// </summary>
		public void Developer( string message )
			=> LogSystem.Developer( Tag, message );

		/// <summary>
		/// Prints a verbose message.
		/// </summary>
		public void Verbose( string message )
			=> LogSystem.Verbose( Tag, message );

		/// <summary>
		/// Prints a success message.
		/// </summary>
		public void Success( string message )
			=> LogSystem.Success( Tag, message );
	}
}
