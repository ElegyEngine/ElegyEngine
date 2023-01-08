
namespace Elegy.Internal
{
	internal class Console
	{
		public Console( string[] args )
		{
			InitialiseArguments( args );
		}

		public bool Init()
		{
			Elegy.Console.SetConsole( this );
			Elegy.Console.Log( "[Console] Init" );

			AddFrontend( new ConsoleFrontends.GodotConsoleFrontend() );

			return true;
		}

		public void Shutdown()
		{
			Elegy.Console.Log( "[Console] Shutdown\n" );

			mFrontends.ForEach( frontend => frontend.Shutdown() );
			mFrontends.Clear();

			mArguments.Clear();
		}

		public void Log( string message, ConsoleMessageType type = ConsoleMessageType.Info )
		{
			if ( type == ConsoleMessageType.Developer && !Elegy.Console.Developer )
			{
				return;
			}
			if ( type == ConsoleMessageType.Verbose && !Elegy.Console.Verbose )
			{
				return;
			}

			for ( int i = 0; i < mFrontends.Count; i++ )
			{
				mFrontends[i].OnLog( message, type );
			}
		}

		public bool AddFrontend( IConsoleFrontend frontend )
		{
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
				Elegy.Console.Log( $"[Console] Added frontend '{frontend.Name}'", ConsoleMessageType.Developer );
				return true;
			}

			Elegy.Console.Warning( $"[Console] '{frontend.Name}' failed to initialise with message: '{frontend.Error}'" );
			return false;
		}

		private void InitialiseArguments( string[] args )
		{
			if ( args.Length == 0 )
			{
				Elegy.Console.Log( "[Console] Launch arguments: empty", ConsoleMessageType.Verbose );
				return;
			}

			var isKey = ( string text ) => text.StartsWith( "-" ) || text.StartsWith( "+" );

			Elegy.Console.Log( "[Console] Launch arguments:", ConsoleMessageType.Verbose );

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
					Elegy.Console.Log( $"    * '{args[i]}' = '{value}'", ConsoleMessageType.Verbose );
				}
			}
		}

		private List<IConsoleFrontend> mFrontends = new();
		private StringDictionary mArguments = new();
	}
}
