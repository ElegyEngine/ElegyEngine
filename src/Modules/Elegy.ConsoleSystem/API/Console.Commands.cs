// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem.Commands;

namespace Elegy.ConsoleSystem.API
{
	public static partial class Console
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns><c>true</c> upon success, <c>false</c> upon encountering
		/// a duplicate or other error.</returns>
		public static bool RegisterCommand( ConsoleCommand command )
		{
			if ( mCommands.ContainsKey( command.Name ) )
			{
				mLogger.Warning( $"Tried registering command '{command.Name}', already exists!" );
				return false;
			}

			mCommands.Add( command.Name, command );
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool UnregisterCommand( ConsoleCommand command )
		{
			if ( !mCommands.ContainsKey( command.Name ) )
			{
				mLogger.Warning( $"Tried unregistering command '{command.Name}', but it's not there!" );
				return false;
			}

			mCommands.Remove( command.Name );
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool Execute( string command )
		{
			string commandName = command;
			string[] commandArguments = Array.Empty<string>();

			int firstSpace = command.IndexOf( ' ' );
			if ( firstSpace != -1 )
			{
				// I'd really like to do something along the lines of
				// ReadOnlySpan<char>[] on this, but this'll do for now!
				commandArguments = command.Substring( firstSpace ).Split( ' ', StringSplitOptions.RemoveEmptyEntries );
				commandName = command.Substring( 0, firstSpace );
			}

			if ( !mCommands.ContainsKey( commandName ) )
			{
				mLogger.Warning( $"Command '{commandName}' does not exist!" );
				return false;
			}

			ConsoleCommand concommand = mCommands[commandName];
			if ( !concommand.Validate( commandArguments, out var outMessage ) )
			{
				mLogger.Warning( outMessage );
				return false;
			}

			return concommand.Method( commandArguments );
		}

		/// <summary>
		/// Commandline arguments passed to the launcher.
		/// </summary>
		public static Dictionary<string, string> Arguments
			=> mArguments;
	}
}
