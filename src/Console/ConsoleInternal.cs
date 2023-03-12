// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	internal sealed class ConsoleInternal
	{
		public const string Tag = "Console";

		public ConsoleInternal( string[] args )
		{
			Console.SetConsole( this );

			InitialiseArguments( args );
		}

		public bool Init()
		{
			Console.SetConsole( this );
			Console.Log( Tag, "Init" );

			AddFrontend( new ConsoleFrontends.GodotConsoleFrontend() );

			Console.Developer = mArguments.GetBool( "-developer" );
			Console.Verbose = mArguments.GetBool( "-verbose" );

			if ( Console.Verbose )
			{
				Console.Developer = true;
			}

			return true;
		}

		public void Shutdown()
		{
			Console.Log( Tag, "Shutdown" );

			foreach ( IConsoleFrontend frontend in mFrontends )
			{
				if ( frontend.Initialised )
				{
					frontend.Shutdown();
				}
			}
			mFrontends.Clear();

			mArguments.Clear();
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

			message = message.Replace( "\r", string.Empty );

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
				Console.Log( Tag, $"Frontend '{frontend.Name}' already added", ConsoleMessageType.Verbose );
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
				Console.Log( Tag, $"Added frontend '{frontend.Name}'", ConsoleMessageType.Developer );
				return true;
			}

			Console.Warning( Tag, $"'{frontend.Name}' failed to initialise with message: '{frontend.Error}'" );
			return false;
		}

		public bool RemoveFrontend( IConsoleFrontend frontend )
		{
			if ( !mFrontends.Exists( internalFrontend => internalFrontend == frontend ) )
			{
				Console.Warning( Tag, $"Frontend '{frontend.Name}' is already removed" );
				return false;
			}
			
			if ( frontend.Initialised )
			{
				frontend.Shutdown();
			}

			mFrontends.Remove( frontend );
			Console.Log( Tag, $"Removed frontend '{frontend.Name}'" );
			return true;
		}

		private void InitialiseArguments( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.Log( Tag, "Launch arguments: empty", ConsoleMessageType.Verbose );
				return;
			}

			var isKey = ( string text ) => text.StartsWith( "-" ) || text.StartsWith( "+" );

			Console.Log( Tag, "Launch arguments:", ConsoleMessageType.Verbose );

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
					Console.Log( $"    * '{args[i]}' = '{value}'", ConsoleMessageType.Verbose );
				}
			}
		}

		private List<IConsoleFrontend> mFrontends = new();
		private StringDictionary mArguments = new();
		private string mCurrentMessage = string.Empty;
	}
}
