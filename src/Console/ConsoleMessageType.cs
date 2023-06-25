// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Type of console message. Used by <seealso cref="IConsoleFrontend"/> plugins to
	/// differentiate between different message types.
	/// </summary>
	public enum ConsoleMessageType : byte
	{
		/// <summary>
		/// General messages.
		/// </summary>
		Info,

		/// <summary>
		/// Outstanding success messages.
		/// In external consoles, these messages would
		/// be displayed in green.
		/// </summary>
		Success,

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
