// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Bootstrap;
using Elegy.PlatformSystem.API;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.API;

using Silk.NET.Windowing;
using System.Diagnostics;

using IView = Elegy.RenderSystem.Interfaces.Rendering.IView;

namespace Elegy.Launcher2
{
	using Engine = Engine.Engine;
	
	internal static class Program
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

		private static Stopwatch mStopwatch = new();
		static double GetSeconds() => (double)mStopwatch.ElapsedTicks / Stopwatch.Frequency;

		/// <summary>
		/// Kicks off the update loop.
		/// </summary>
		static void Run()
		{
			if ( Engine.IsHeadless )
			{
				RunHeadless();
				return;
			}

			IWindow window = Platform.GetCurrentWindow();
			IView? renderView = Render.Instance.GetView( window );
			Debug.Assert( renderView is not null );

			window.Update += ( deltaTime ) =>
			{
				Engine.Update( (float)deltaTime );

				if ( !Engine.IsRunning )
				{
					window.Close();
				}
			};

			window.Render += ( deltaTime ) =>
			{
				if ( window.CanSwap() )
				{
					Render.RenderFrame( renderView );
				}
			};

			window.Run();
		}

		/// <summary>
		/// Kicks off the update loop without rendering nor input.
		/// </summary>
		static void RunHeadless()
		{
			double lastTime = -0.016;

			while ( Engine.IsRunning )
			{
				double deltaTime = GetSeconds() - lastTime;
				Engine.Update( (float)deltaTime );
			}
		}

		[ElegyMain]
		[WithAllGameSystems]
		static void Main( string[] args )
		{
			Console.Title = "Elegy.Launcher2";
			mStopwatch.Restart();

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
			Run();
		}
	}
}