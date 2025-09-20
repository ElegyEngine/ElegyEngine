// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.CommandSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Interfaces;
using Elegy.Common.Utilities;
using Elegy.Core;
using Elegy.InputSystem.API;
using Elegy.PlatformSystem.API;
using Elegy.PluginSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Silk.NET.Windowing;

namespace Elegy.App;

public enum AppStages
{
	AssetSystemGlue,
	AppGlue,
	PlatformAndInputSystem,
	RenderSystem,
	GraphicsDevice,
	LoadMaterialTemplates,
	GraphicsResources,
	CreateWindow,
	StartApps
}

public static partial class AppTemplate
{
	private static TaggedLogger mLogger = new( "App" );

	private static IApplication? mApplication;
	private static IWindowPlatform? mWindowPlatform;
	private static View? mRenderView;

	/// <summary>
	/// Creates a startup <see cref="Orchestrator{T}"/> with the full set of engine systems.
	/// </summary>
	public static Orchestrator<LaunchConfig> CreateStartupOrchestrator()
		=> CoreTemplate.CreateStartupOrchestrator()
			// Before any assets are loaded, the asset system should use the
			// render system's resource factories for materials and textures
			.After( CoreStages.AssetSystem, AppStages.AssetSystemGlue, AssetGlueSetup )
			// Before plugins get loaded, register all needed plugin collectors etc.
			.Before( CoreStages.PluginActivation, orc => orc
				.Add( AppStages.PlatformAndInputSystem, InitPlatformAndInput, ShutdownPlatformAndInput )
				// Basic renderer init and shutdown here
				.Add( AppStages.RenderSystem, Render.Init, Render.Shutdown )
				.Add( AppStages.AppGlue, AppGlueSetup, AppGlueCleanup ) )
			// Create graphics device here, so DoLoadMaterialTemplates can use it.
			// Any plugins for the render system should also be loaded at this point
			.After( CoreStages.PluginActivation, AppStages.GraphicsDevice, Render.CreateGraphicsDevice )
			// Now that all the other subsystems are ready, prepare everything needed for
			// an interactive app, like creating a window, begin sampling input etc.
			.After( CoreStages.FinalStep, orc => orc
				// Load shaders, material templates etc.
				.Add( AppStages.LoadMaterialTemplates, DoLoadMaterialTemplates )
				// Create missing texture, builtin meshes, textures etc.
				.Add( AppStages.GraphicsResources, Render.CreateGraphics )
				// Now that there is a graphics device, create a window and the associated view for it
				.Add( AppStages.CreateWindow, DoCreateWindow )
				// TODO: Is the ability to start 2 and more apps useful? How would that work?
				.Add( AppStages.StartApps, DoStartApps ) );

	private static bool AssetGlueSetup( LaunchConfig config )
	{
		Assets.SetRenderFactories( Render.CreateMaterial,
			( textureInfo, data ) => Render.CreateTexture( textureInfo, data.AsSpan() ) );
		return true;
	}

	private static bool AppGlueSetup( LaunchConfig config )
	{
		Platform.Set( mWindowPlatform );

		Plugins.RegisterDependency( "Elegy.InputSystem", typeof( Input ).Assembly );
		Plugins.RegisterDependency( "Elegy.PlatformSystem", typeof( Platform ).Assembly );
		Plugins.RegisterDependency( "Elegy.RenderBackend", typeof( RenderBackend.Utils ).Assembly );
		Plugins.RegisterDependency( "Elegy.RenderSystem", typeof( Render ).Assembly );
		Plugins.RegisterPluginCollector<RenderPluginCollector>();

		return true;
	}

	private static void AppGlueCleanup()
	{
		Plugins.UnregisterPluginCollector<RenderPluginCollector>();
		Plugins.UnregisterDependency( "Elegy.InputSystem" );
		Plugins.UnregisterDependency( "Elegy.PlatformSystem" );
		Plugins.UnregisterDependency( "Elegy.RenderBackend" );
		Plugins.UnregisterDependency( "Elegy.RenderSystem" );
	}

	private static bool InitPlatformAndInput( LaunchConfig config )
	{
		return Platform.Init() && Input.Init();
	}

	private static void ShutdownPlatformAndInput()
	{
		Input.Shutdown();
		Platform.Shutdown();
	}

	private static bool DoCreateWindow( LaunchConfig config )
	{
		Headless = Commands.Arguments.GetBool( "-headless" );

		if ( Headless )
		{
			return true;
		}

		if ( config.WithMainWindow )
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
					Size = new( 1600, 900 ),
					VSync = true
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

	private static bool DoStartApps( LaunchConfig config )
	{
		List<string> failedPlugins = new();
		foreach ( var app in Plugins.Applications )
		{
			if ( !app.Start() )
			{
				failedPlugins.Add( $"'{app.Name}' - failed to start ({app.Error})" );
			}
		}

		if ( failedPlugins.Count > 0 )
		{
			mLogger.Error( "Applications failed to load:" );
			for ( int i = 0; i < failedPlugins.Count; i++ )
			{
				mLogger.Log( $" * {failedPlugins[i]}" );
			}

			mLogger.Log( "Resolve these application errors and try again." );
			return false;
		}

		return true;
	}
}
