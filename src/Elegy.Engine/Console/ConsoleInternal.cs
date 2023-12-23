// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.ConsoleCommands;
using Elegy.ConsoleCommands.Helpers;

namespace Elegy
{
	internal sealed class ConsoleInternal
	{
		private TaggedLogger mLogger = new( "Console" );
		
		public ConsoleInternal( string[] args )
		{
			Console.SetConsole( this );
			InitialiseArguments( args );
		}

		public bool Init()
		{
			Console.SetConsole( this );
			mLogger.Log( "Init" );

			AddFrontend( new ConsoleFrontends.GodotConsoleFrontend() );

			Console.Verbose = mArguments.GetBool( "-verbose" );
			Console.Developer = Console.Verbose || mArguments.GetBool( "-developer" );

			// Register the builtin argument helpers so they can
			// be used in mEngineConvarRegistry
			Assembly engineAssembly = Assembly.GetExecutingAssembly();
			HelperManager.RegisterHelpers( engineAssembly );

			mEngineConvarRegistry = new( engineAssembly );
			mEngineConvarRegistry.RegisterAll();
			return true;
		}

		public void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			foreach ( IConsoleFrontend frontend in mFrontends )
			{
				if ( frontend.Initialised )
				{
					frontend.Shutdown();
				}
			}
			mFrontends.Clear();
			mArguments.Clear();

			mEngineConvarRegistry.UnregisterAll();
		}

		public void Log( string message, ConsoleMessageType type = ConsoleMessageType.Info )
		{
			if ( type == ConsoleMessageType.Developer && !Console.Developer )
			{
				return;
			}
			if ( type == ConsoleMessageType.Verbose && !Console.Verbose )
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

			float timeSubmitted = Time.GetTicksMsec() * 0.001f;
			for ( int i = 0; i < stringsToSend; i++ )
			{
				LogToFrontends( messageLines[i], type, timeSubmitted );
			}
		}

		private void LogToFrontends( string message, ConsoleMessageType type, float timeSubmitted )
		{
			for ( int i = 0; i < mFrontends.Count; i++ )
			{
				mFrontends[i].OnLog( message, type, timeSubmitted );
			}
		}

		public void Update( float delta )
		{
			for ( int i = 0; i < mFrontends.Count; i++ )
			{
				mFrontends[i].OnUpdate( delta );
			}
		}

		public bool AddFrontend( IConsoleFrontend frontend )
		{
			if ( mFrontends.Contains( frontend ) )
			{
				mLogger.Verbose( $"Frontend '{frontend.Name}' already added" );
				return true;
			}

			// Since console frontends are plugins, they can be often times initialised by the plugin system
			// Sometimes, however, somebody may simply call Console.AddFrontend( new MyFrontend() );, in
			// which case we initialise it here.
			if ( !frontend.Initialised )
			{
				frontend.Init();
			}

			if ( frontend.Error == string.Empty )
			{
				mFrontends.Add( frontend );
				mLogger.Developer( $"Added frontend '{frontend.Name}'" );
				return true;
			}

			mLogger.Warning( $"'{frontend.Name}' failed to initialise with message: '{frontend.Error}'" );
			return false;
		}

		public bool RemoveFrontend( IConsoleFrontend frontend )
		{
			if ( !mFrontends.Exists( internalFrontend => internalFrontend == frontend ) )
			{
				mLogger.Warning( $"Frontend '{frontend.Name}' is already removed" );
				return false;
			}
			
			if ( frontend.Initialised )
			{
				frontend.Shutdown();
			}

			mFrontends.Remove( frontend );
			mLogger.Log( $"Removed frontend '{frontend.Name}'" );
			return true;
		}

		public bool RegisterCommand( ConsoleCommand command )
		{
			if ( mCommands.ContainsKey( command.Name ) )
			{
				mLogger.Warning( $"Tried registering command '{command.Name}', already exists!" );
				return false;
			}

			mCommands.Add( command.Name, command );
			return true;
		}

		public bool UnregisterCommand( ConsoleCommand command )
		{
			if ( !mCommands.ContainsKey( command.Name ) )
			{
				mLogger.Warning( $"Tried unregistering command '{command.Name}', but it's not there!" );
				return false;
			}

			mCommands.Remove( command.Name );
			return true;
		}

		public bool Execute( string command )
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
				Console.Warning( outMessage );
				return false;
			}
			
			return concommand.Method( commandArguments );
		}

		private void InitialiseArguments( string[] args )
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

		private ConVarRegistry mEngineConvarRegistry;
		private List<IConsoleFrontend> mFrontends = new();
		private StringDictionary mArguments = new();
		private Dictionary<string, ConsoleCommand> mCommands = new();
		private string mCurrentMessage = string.Empty;
	}
}
