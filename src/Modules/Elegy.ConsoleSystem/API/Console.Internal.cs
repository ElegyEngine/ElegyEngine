// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem.Commands;
using Elegy.ConsoleSystem.Frontends;
using System.Diagnostics;

namespace Elegy.ConsoleSystem.API
{
	public static partial class Console
	{
		private static TaggedLogger mLogger = new( "Console" );
		private static ConVarRegistry? mEngineConvarRegistry = null;
		private static Stopwatch mTimer = Stopwatch.StartNew();
		private static List<IConsoleFrontend> mFrontends = new();
		private static Dictionary<string, string> mArguments = new();
		private static Dictionary<string, ConsoleCommand> mCommands = new();
		private static string mCurrentMessage = string.Empty;

		internal static void LogInternal( string message, ConsoleMessageType type = ConsoleMessageType.Info )
		{
			if ( type == ConsoleMessageType.Fatal )
			{
				Debugger.Break();
			}

			if ( type == ConsoleMessageType.Developer && !Developer )
			{
				return;
			}
			if ( type == ConsoleMessageType.Verbose && !Verbose )
			{
				return;
			}

			message = message.Replace( '\r', ' ' );

			// Message logic is a tad complicated, see below
			mCurrentMessage += message;

			// If there are no newlines, means we're still inline,
			// hoping for a newline in the next message. So, store
			// the text until then.
			if ( !mCurrentMessage.Contains( '\n' ) )
			{
				return;
			}

			// At this stage, we have at least one newline
			// The message could take any shape or form:
			// "abc\ndef" -> "abc" and "def", store "def" for later
			// "abc\n\ndef" -> "abc", "" and "def", store "def" for later
			// "abc\ndef\n" -> "abc" and "def", send both
			// "abcdef\n" -> "abcdef"
			// "\nabcdef\n" -> "" and "abcdef"
			// "abcdef" -> store "abcdef" for later
			var messageLines = mCurrentMessage.Split( '\n' );

			// -1 because "abc\n" will be an array of 2 strings: "abc" and ""
			int stringsToSend = messageLines.Length - 1;

			// Save the line that doesn't end with \n so we can inline it later
			if ( !mCurrentMessage.EndsWith( "\n" ) )
			{
				mCurrentMessage = messageLines[messageLines.Length - 1];
			}
			// If the message does end with a newline, then we got nothing to save
			else
			{
				mCurrentMessage = string.Empty;
			}

			float timeSubmitted = (float)mTimer.ElapsedTicks / Stopwatch.Frequency;
			for ( int i = 0; i < stringsToSend; i++ )
			{
				LogToFrontends( messageLines[i], type, timeSubmitted );
			}
		}

		private static void LogToFrontends( string message, ConsoleMessageType type, float timeSubmitted )
		{
			for ( int i = 0; i < mFrontends.Count; i++ )
			{
				mFrontends[i].OnLog( message, type, timeSubmitted );
			}
		}

		private static void InitialiseArguments( string[] args )
		{
			if ( args.Length == 0 )
			{
				mLogger.Verbose( "Launch arguments: empty" );
				return;
			}

			var isKey = ( string text ) => text.StartsWith( "-" ) || text.StartsWith( "+" );

			mLogger.Verbose( "Launch arguments:" );

			for ( int i = 0; i < args.Length; i++ )
			{
				if ( isKey( args[i] ) )
				{
					string value = "1";

					if ( i < args.Length - 1 && !isKey( args[i + 1] ) )
					{
						value = args[i + 1];
					}

					mArguments[args[i]] = value;
					mLogger.Verbose( $"   * '{args[i]}' = '{value}'" );
				}
			}
		}
	}
}
