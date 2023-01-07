
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
			mFrontends.Add( new ConsoleFrontends.GodotConsoleFrontend() );
			Log( "Console.Init" );
			return true;
		}

		public void Shutdown()
		{
			Log( "Console.Shutdown" );

			mFrontends.ForEach( frontend => frontend.Shutdown() );
			mFrontends.Clear();

			mArguments.Clear();
		}

		public void Log( string message )
		{
			Submit( message, ConsoleMessageType.Info );
		}

		public void Warning( string message )
		{
			Submit( message, ConsoleMessageType.Warning );
		}

		public void Error( string message, bool fatal = false )
		{
			Submit( message, fatal ? ConsoleMessageType.Fatal : ConsoleMessageType.Error );
		}

		private void Submit( string message, ConsoleMessageType type )
		{
			ConsoleMessage messageObject = new()
			{
				Text = message,
				Type = type
			};

			for ( int i = 0; i < mFrontends.Count; i++ )
			{
				mFrontends[i].OnLog( ref messageObject );
			}
		}

		private void InitialiseArguments( string[] args )
		{
			var isKey = ( string text ) => text.StartsWith( "-" ) || text.StartsWith( "+" );

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
				}
			}
		}

		private List<IConsoleFrontend> mFrontends = new();
		private StringDictionary mArguments = new();
	}
}
