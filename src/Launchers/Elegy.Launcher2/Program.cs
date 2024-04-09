// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Bootstrap;
using Elegy.PlatformSystem.API;

using Silk.NET.Windowing;

namespace Elegy.Launcher2
{
	using Engine = Engine.Engine;
	
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

		[ElegyMain]
		[WithAllGameSystems]
		static void Main( string[] args )
		{
			Console.Title = "Elegy.Launcher2";

			LaunchConfig config = new()
			{
				Args = args,
				EngineConfigName = "engineConfig.json",
				WithMainWindow = true
			};

			while ( !Engine.Init( config ) )
			{
				if ( Engine.ShutdownReason is null )
				{
					PrintError( $"Engine failed to initialise: reason unknown" );
				}
				else
				{
					PrintError( $"Engine failed to initialise: '{Engine.ShutdownReason}'" );
				}

				Console.WriteLine( "Press ESC to exit. Any other key will retry." );
				if ( Console.ReadKey().Key == ConsoleKey.Escape )
				{
					return;
				}
			}

			Platform.Set( GetWindowPlatform() );

			// TODO: Run engine and render frames
		}
	}
}