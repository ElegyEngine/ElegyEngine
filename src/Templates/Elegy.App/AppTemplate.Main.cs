// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.Core;
using Elegy.LogSystem.API;
using Elegy.PlatformSystem.API;
using Elegy.PluginSystem.API;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Silk.NET.Windowing;

namespace Elegy.App;

// The main user API is here
public static partial class AppTemplate
{
	/// <summary>
	/// Headless mode is basically server mode. There's no window, it just simulates any app logic.
	/// </summary>
	public static bool Headless { get; private set; }

	/// <summary>
	/// Whether the engine is currently running and actively updating itself.
	/// </summary>
	public static bool Running { get; private set; }

	/// <summary>
	/// Seconds since the program started.
	/// </summary>
	public static double GetSeconds() => CoreTemplate.GetSeconds();

	/// <summary>
	/// Bootstraps all subsystems and starts the engine. Also handles the main loop.
	/// </summary>
	public static void Start( LaunchConfig config, IWindowPlatform? windowPlatform )
	{
		mWindowPlatform = windowPlatform;

		Run( config, SimpleUpdateLoop );
	}

	/// <inheritdoc cref="Start"/>
	public static void Start<T>( LaunchConfig config, IWindowPlatform? windowPlatform )
		where T : IApplication, new()
	{
		config.ToolMode = true;
		mApplication = new T();
		mWindowPlatform = windowPlatform;

		Run( config, SimpleUpdateLoop );
	}

	/// <summary>
	/// Initialises all subsystems, does not handle the main loop. Intended for
	/// situations where more fine-grained control is required (e.g. integrating
	/// into desktop UI frameworks).
	/// </summary>
	public static bool Init( LaunchConfig config, IWindowPlatform? windowPlatform )
	{
		mWindowPlatform = windowPlatform;
		Running = true;

		return CreateStartupOrchestrator().Init( config );
	}

	/// <inheritdoc cref="Init"/>
	public static bool Init<T>( LaunchConfig config, IWindowPlatform? windowPlatform )
		where T : IApplication, new()
	{
		config.ToolMode = true;
		mWindowPlatform = windowPlatform;
		Running = true;

		return CreateStartupOrchestrator().Init( config );
	}

	/// <summary>
	/// Shuts down all subsystems. Intended for situations where more fine-grained
	/// control is required (e.g. integrating into desktop UI frameworks).
	/// </summary>
	public static void Shutdown()
		=> CreateStartupOrchestrator().Shutdown();

	/// <summary>
	/// Updates engine systems and running apps, essentially performing a frame.
	/// This is automatically called by <see cref="Start"/>, but if you want manual
	/// control, you can call this between <see cref="Init"/> and <see cref="Shutdown"/>.
	/// </summary>
	public static void Update( double deltaTime )
	{
		Log.Update( (float)deltaTime );

		foreach ( var app in Plugins.Applications )
		{
			if ( !app.RunFrame( (float)deltaTime ) )
			{
				Plugins.UnloadApplication( app );
			}
		}

		if ( Plugins.Applications.Count == 0 )
		{
			Running = false;
		}
	}

	private static bool Run( LaunchConfig config, Action main )
	{
		Running = true;
		if ( mApplication is not null )
		{
			Plugins.RegisterPlugin( mApplication );
		}

		return CreateStartupOrchestrator().Run( config, main );
	}

	private static void LoopHeadless()
	{
		double lastTime = -0.016;

		while ( Running )
		{
			double deltaTime = GetSeconds() - lastTime;
			Update( deltaTime );
		}
	}

	private static void SimpleUpdateLoop()
	{
		if ( Headless )
		{
			LoopHeadless();
			return;
		}

		IWindow? window = Platform.GetCurrentWindow();
		Debug.Assert( window is not null );

		View? renderView = Render.GetView( window );
		Debug.Assert( renderView is not null );

		// TODO: isolate the update loop into its own thing,
		// but keep rendering to the window's render loop
		window.Update += deltaTime =>
		{
			Update( deltaTime );

			if ( !Running )
			{
				window.Close();
			}
		};

		window.Render += _ =>
		{
			if ( window.CanSwap() )
			{
				Render.RenderFrame( renderView );
			}
		};

		// Shutdown should be implicit now I reckon
		//window.Closing += () => { EngineSystem.Shutdown(); };

		window.Run();
	}
}
