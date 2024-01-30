
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Elegy.Launcher2
{
	internal class Program
	{
		static void PrintError( string message )
		{
			System.Console.BackgroundColor = ConsoleColor.Red;
			System.Console.ForegroundColor = ConsoleColor.Black;
			System.Console.WriteLine( message );
			System.Console.BackgroundColor = ConsoleColor.Black;
			System.Console.ForegroundColor = ConsoleColor.White;
		}

		static IWindowPlatform? GetWindowPlatform()
		{
			Window.PrioritizeSdl();
			if ( Window.GetWindowPlatform( false )?.Name.ToLower().Contains( "sdl" ) ?? false )
			{
				return null;
			}


		}

		static void Main( string[] args )
		{
			System.Console.Title = "Elegy.Launcher2";

			Engine engine = new( args, GetWindowPlatform() );

			while ( !engine.Init() )
			{
				if ( engine.ShutdownReason is null )
				{
					PrintError( $"Engine failed to initialise: reason unknown" );
				}
				else
				{
					PrintError( $"Engine failed to initialise: '{engine.ShutdownReason}'" );
				}

				System.Console.WriteLine( "Press ESC to exit. Any other key will retry." );
				if ( System.Console.ReadKey().Key == ConsoleKey.Escape )
				{
					return;
				}
			}

			engine.Run();
		}
	}
}