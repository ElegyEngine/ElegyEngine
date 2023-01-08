﻿
namespace Elegy
{
	public enum ConsoleMessageType
	{
		/// <summary>
		/// General messages.
		/// </summary>
		Info,

		/// <summary>
		/// Developer messages.
		/// </summary>
		Developer,

		/// <summary>
		/// Verbose developer messages.
		/// </summary>
		Verbose,

		/// <summary>
		/// Warning message. There was a minor fault.
		/// </summary>
		Warning,
		
		/// <summary>
		/// Error message. There was a more major fault, but
		/// the system can recover from it.
		/// </summary>
		Error,

		/// <summary>
		/// Irrecoverable error message.
		/// </summary>
		Fatal
	}
}
