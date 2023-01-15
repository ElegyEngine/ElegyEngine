// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Utilities;

namespace Elegy.Internal
{
	internal sealed class ConsoleInternal
	{
		public ConsoleInternal( string[] args )
		{
			Console.SetConsole( this );

			InitialiseArguments( args );
		}

		public bool Init()
		{
			Console.SetConsole( this );
			Console.Log( "[Console] Init" );

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
			Console.Log( "[Console] Shutdown" );

			mFrontends.ForEach( frontend => frontend.Shutdown() );
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
				Console.Log( $"[Console] Added frontend '{frontend.Name}'", ConsoleMessageType.Developer );
				return true;
			}

			Console.Warning( $"[Console] '{frontend.Name}' failed to initialise with message: '{frontend.Error}'" );
			return false;
		}

		public bool RemoveFrontend( IConsoleFrontend frontend )
		{
			if ( !mFrontends.Exists( internalFrontend => internalFrontend == frontend ) )
			{
				Console.Warning( $"[Console] Frontend '{frontend.Name}' is already removed" );
				return false;
			}
			
			if ( frontend.Initialised )
			{
				frontend.Shutdown();
			}

			mFrontends.Remove( frontend );
			Console.Log( $"[Console] Removed frontend '{frontend.Name}'" );
			return true;
		}

		private void InitialiseArguments( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.Log( "[Console] Launch arguments: empty", ConsoleMessageType.Verbose );
				return;
			}

			var isKey = ( string text ) => text.StartsWith( "-" ) || text.StartsWith( "+" );

			Console.Log( "[Console] Launch arguments:", ConsoleMessageType.Verbose );

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
	}
}
