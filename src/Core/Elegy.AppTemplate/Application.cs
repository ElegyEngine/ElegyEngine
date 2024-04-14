// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;
using Elegy.Framework;
using Elegy.Framework.Bootstrap;
using Elegy.PlatformSystem.API;
using Elegy.PluginSystem.API;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;

using Silk.NET.Windowing;
using System.Diagnostics;

using IView = Elegy.RenderSystem.Interfaces.Rendering.IView;
using ElegyConsole = Elegy.ConsoleSystem.API.Console;

namespace Elegy.AppTemplate
{
	[ElegyBootstrap]
	[WithAssetSystem]
	[WithConsoleSystem]
	[WithFileSystem]
	[WithInputSystem]
	[WithPlatformSystem]
	[WithPluginSystem]
	[WithRenderSystem]
	public static partial class Application
	{
		private static IView? mRenderView = null;
		private static TaggedLogger mLogger = new( "App" );
		private static Stopwatch mStopwatch = new();
		public static double GetSeconds() => (double)mStopwatch.ElapsedTicks / Stopwatch.Frequency;

		static void PrintError( string message )
		{
			Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.WriteLine( message );
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
		}

		/// <summary>
		/// Updates the engine subsystems.
		/// </summary>
		public static void Update( double deltaTime )
		{
			ElegyConsole.Update( (float)deltaTime );

			foreach ( var app in Plugins.Applications )
			{
				if ( !app.RunFrame( (float)deltaTime ) )
				{
					Plugins.UnloadApplication( app );
				}
			}

			if ( Plugins.Applications.Count == 0 )
			{
				EngineSystem.Shutdown( "" );
			}
		}

		/// <summary>
		/// Is the engine running in headless/dedicated server mode?
		/// </summary>
		public static bool IsHeadless { get; private set; } = false;

		/// <summary>
		/// Kicks off the update loop.
		/// </summary>
		public static void Run()
		{
			if ( IsHeadless )
			{
				RunHeadless();
				return;
			}

			IWindow? window = Platform.GetCurrentWindow();
			Debug.Assert( window is not null );

			IView? renderView = Render.Instance.GetView( window );
			Debug.Assert( renderView is not null );

			window.Update += ( deltaTime ) =>
			{
				Update( deltaTime );

				if ( !EngineSystem.IsRunning )
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

			window.Closing += () =>
			{
				EngineSystem.Shutdown();
			};

			window.Run();
		}

		/// <summary>
		/// Kicks off the update loop without rendering nor input.
		/// </summary>
		public static void RunHeadless()
		{
			double lastTime = -0.016;

			while ( EngineSystem.IsRunning )
			{
				double deltaTime = GetSeconds() - lastTime;
				Update( deltaTime );
			}
		}

		/// <summary>
		/// Does exactly what it says.
		/// </summary>
		public static bool InitialiseWindowAndRenderer( bool initialiseDefaultWindow )
		{
			if ( IsHeadless )
			{
				return true;
			}

			if ( initialiseDefaultWindow )
			{
				// Now that the engine is mostly initialised, we can create a window for the application
				// Assuming, of course, this isn't a headless instance, and a window isn't already provided
				if ( Platform.GetCurrentWindow() is null )
				{
					IWindow? window = Platform.CreateWindow( new()
					{
						API = GraphicsAPI.DefaultVulkan,
						FramesPerSecond = 120.0,
						UpdatesPerSecond = 120.0,
						Size = new( 1600, 900 )
					} );

					if ( window is null )
					{
						mLogger.Error( "Cannot create window!" );
						return false;
					}
				}
			}

			mRenderView = Render.GetOrCreateDefaultView( 1600, 900, rate: 120 );
			return mRenderView is not null;
		}

		/// <summary>
		/// Bootstraps all subsystems and starts the engine. Does not start the main loop.
		/// </summary>
		public static bool Init( LaunchConfig config, IWindowPlatform? windowPlatform )
		{
			Stopwatch startupTimer = Stopwatch.StartNew();

			while ( !EngineSystem.Init( config,
				// These four get filled in by Elegy.Framework.Generator,
				// don't worry about them erroring out! It's a bit like magic
				systemInitFunc: Init_Generated,
				systemPostInitFunc: PostInit_Generated,
				systemShutdownFunc: Shutdown_Generated,
				systemErrorFunc: ErrorMessage_Generated ) )
			{
				if ( EngineSystem.ShutdownReason is null )
				{
					PrintError( $"Engine failed to initialise: reason unknown" );
				}
				else
				{
					PrintError( $"Engine failed to initialise: '{EngineSystem.ShutdownReason}'" );
				}

				Console.WriteLine( "Press ESC to exit. Any other key will retry." );
				if ( Console.ReadKey().Key == ConsoleKey.Escape )
				{
					return false;
				}

				startupTimer.Restart();
			}

			IsHeadless = ElegyConsole.Arguments.GetBool( "headless" );
			Platform.Set( windowPlatform );

			if ( !InitialiseWindowAndRenderer( config.WithMainWindow ) )
			{
				mLogger.Error( "Couldn't initialise rendering" );
				EngineSystem.Shutdown();
				return false;
			}

			if ( !Plugins.StartApps() )
			{
				mLogger.Error( "Failed to start app(s)" );
				EngineSystem.Shutdown();
				return false;
			}

			mLogger.Success( string.Format( "Startup time: {0:F}s elapsed", (double)startupTimer.ElapsedTicks / Stopwatch.Frequency ) );

			return true;
		}

		/// <summary>
		/// Bootstraps all subsystems and starts the engine. Also handles the main loop.
		/// </summary>
		public static void Start( LaunchConfig config, IWindowPlatform windowPlatform )
		{
			if ( !Init( config, windowPlatform ) )
			{
				return;
			}

			Run();
		}
	}
}
