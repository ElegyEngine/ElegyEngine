// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Windowing;

namespace Elegy.Launcher2
{
	internal class Program
	{
		static void PrintError( string message )
		{
			Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.WriteLine( message );
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
		}

		static IWindowPlatform? GetWindowPlatform()
		{
			Window.PrioritizeSdl();
			IWindowPlatform? windowPlatform = Window.GetWindowPlatform( false );
			if ( windowPlatform is null )
			{
				return null;
			}

			if ( !windowPlatform.Name.ToLower().Contains( "sdl" ) )
			{
				return null;
			}

			return windowPlatform;
		}

		static void Main( string[] args )
		{
			Console.Title = "Elegy.Launcher2";

			Engine.Engine engine = new( args, GetWindowPlatform() );

			while ( !engine.Init( withMainWindow: true ) )
			{
				if ( engine.ShutdownReason is null )
				{
					PrintError( $"Engine failed to initialise: reason unknown" );
				}
				else
				{
					PrintError( $"Engine failed to initialise: '{engine.ShutdownReason}'" );
				}

				Console.WriteLine( "Press ESC to exit. Any other key will retry." );
				if ( Console.ReadKey().Key == ConsoleKey.Escape )
				{
					return;
				}
			}

			engine.Run();
		}
	}
}