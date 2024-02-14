// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using Elegy.RenderBackend.Extensions;
using Silk.NET.Windowing;
using System.Diagnostics;

using IView = Elegy.Rendering.IView;

namespace Elegy
{
	/// <summary>
	/// The engine. Launches and updates all subsystems.
	/// </summary>
	public partial class Engine
	{
		/// <summary>
		/// Elegy Engine major version, used for version checking against plugins.
		/// </summary>
		public const int MajorVersion = 0;
		/// <summary>
		/// Elegy Engine minor version, used for version checking against plugins.
		/// </summary>
		public const int MinorVersion = 1;
		/// <summary>
		/// Plugins built before this minor version will not work.
		/// </summary>
		public const int OldestSupportedMinor = 0;

		/// <summary>
		/// Version string to be displayed when the engine starts up.
		/// </summary>
		public static readonly string VersionString = $"{MajorVersion}.{MinorVersion}";

		/// <summary>
		/// The reason of shutdown. Might be an error.
		/// </summary>
		public string? ShutdownReason { get; private set; } = null;

		/// <summary>
		/// Time the engine started.
		/// </summary>
		public static DateTime StartupTime { get; private set; }

		private TaggedLogger mLogger = new( "Engine" );

		/// <summary>
		/// One and only engine constructor.
		/// </summary>
		public Engine( string[] args, IWindowPlatform? windowPlatform )
		{
			StartupTime = DateTime.Now;
			
			mCommandlineArgs = args;
			mWindowPlatform = windowPlatform;
		}

		/// <summary>
		/// Initialises the engine's systems.
		/// </summary>
		public bool Init( IConsoleFrontend? extraFrontend = null )
		{
			mHasShutdown = false;

			mCore = new( Stopwatch.StartNew(), mWindowPlatform );
			Core.SetCore( mCore );

			if ( !InitialiseConsole( extraFrontend ) )
			{
				return Shutdown( "Console system failure" );
			}

			mCore.IsHeadless = Console.Arguments.GetBool( "headless" );

			if ( !LoadOrCreateEngineConfig( "engineConfig.json" ) )
			{
				return Shutdown( "Configuration failure" );
			}

			if ( mEngineConfig.ConfigName != null )
			{
				mLogger.Developer( $"Engine config: '{mEngineConfig.ConfigName}'" );
			}

			mFileSystem = new( mEngineConfig );
			if ( !mFileSystem.Init() )
			{
				return Shutdown( "File system failure" );
			}

			mPluginSystem = new();
			if ( !mPluginSystem.Init() )
			{
				return Shutdown( "Plugin system failure" );
			}

			foreach ( IPlugin plugin in mPluginSystem.GenericPlugins )
			{
				if ( plugin is IConsoleFrontend )
				{
					Console.AddFrontend( plugin as IConsoleFrontend );
				}
			}

			mMaterialSystem = new();
			if ( !mMaterialSystem.Init() )
			{
				return Shutdown( "Material system failure" );
			}

			if ( !InitialiseWindowAndRenderer() )
			{
				return Shutdown( "Render system failure" );
			}

			mLogger.Log( "Successfully initialised all systems" );
			return true;
		}

		/// <summary>
		/// Kicks off the update loop.
		/// </summary>
		public void Run()
		{
			if ( Core.IsHeadless )
			{
				RunHeadless();
				return;
			}

			IWindow window = Core.GetCurrentWindow();
			IView? renderView = Render.Instance.GetView( window );
			Debug.Assert( renderView is not null );

			window.Update += ( deltaTime ) =>
			{
				Update( (float)deltaTime );

				if ( mHasShutdown )
				{
					window.Close();
				}
			};

			int counter = 0;

			window.Render += ( deltaTime ) =>
			{
				if ( window.CanSwap() )
				{
					RenderFrame( renderView );
				}

				counter++;
				mLogger.Log( $"Frame {counter}" );
			};

			window.Run();
			Shutdown();
		}

		/// <summary>
		/// Kicks off the update loop without rendering nor input.
		/// </summary>
		public void RunHeadless()
		{
			float lastTime = -0.016f;

			while ( !mHasShutdown )
			{
				float deltaTime = Core.SecondsFloat - lastTime;
				Update( deltaTime );
			}
		}

		private bool InitialiseConsole( IConsoleFrontend? extraFrontend = null )
		{
			mConsole = new( mCommandlineArgs );
			if ( !mConsole.Init( extraFrontend ) )
			{
				return false;
			}

			Console.Log( $"Initialising Elegy Engine ({VersionString})" );

			if ( MajorVersion < 1 )
			{
				mLogger.Warning( "This is an early in-development build of the engine. DO NOT use in production!" );
			}

			mLogger.Verbose( $"Working directory: '{Directory.GetCurrentDirectory()}'" );
			return true;
		}

		private bool InitialiseWindowAndRenderer()
		{
			if ( Core.IsHeadless )
			{
				// TODO: use dummy render frontend
			}

			IWindow? window = Core.GetCurrentWindow();

			// Now that the engine is mostly initialised, we can create a window for the application
			// Assuming, of course, this isn't a headless instance, and a window isn't already provided
			if ( window is WindowNull )
			{
				window = Core.CreateWindow( new()
				{
					API = GraphicsAPI.DefaultVulkan,
					FramesPerSecond = 120.0,
					UpdatesPerSecond = 120.0,
					Size = new( 320, 240 )
				} );

				if ( window is null )
				{
					mLogger.Error( "Cannot create window!" );
					return false;
				}
			}

			string renderFrontendPath = FileSystem.CurrentConfig.RenderFrontend;

			IPlugin? renderFrontendPlugin = Plugins.LoadPlugin( renderFrontendPath );
			if ( renderFrontendPlugin is null )
			{
				mLogger.Error( "Cannot load render frontend plugin" );
				return false;
			}

			mRenderFrontend = renderFrontendPlugin as IRenderFrontend;
			if ( mRenderFrontend is null )
			{
				mLogger.Error( $"Render frontend plugin '{renderFrontendPath}' doesn't actually implement an IRenderFrontend!" );
				return false;
			}

			mRenderView = mRenderFrontend.CreateView( window );
			Render.SetRenderFrontend( mRenderFrontend );

			return true;
		}

		private bool LoadOrCreateEngineConfig( string path )
		{
			if ( !File.Exists( path ) )
			{
				mLogger.Log( $"'{path}' does not exist, creating a default one..." );

				mEngineConfig = new();
				Text.JsonHelpers.Write( mEngineConfig, path );
				return true;
			}

			if ( !Text.JsonHelpers.LoadFrom( ref mEngineConfig, path ) )
			{
				mLogger.Error( $"'{path}' somehow failed to load" );
				return false;
			}

			return true;
		}

		/// <summary>
		/// Shuts down the engine.
		/// </summary>
		/// <param name="why">The reason of shutdown. If left empty, it is a normal shutdown, else an error.</param>
		/// <returns></returns>
		private bool Shutdown( string why = "" )
		{
			if ( mHasShutdown )
			{
				return true;
			}

			if ( why == "" )
			{
				mLogger.Log( "Shutting down normally..." );
			}
			else
			{
				mLogger.Error( $"Shutting down, reason: {why}" );
			}

			mRenderFrontend?.Shutdown();
			Render.SetRenderFrontend( null );

			mMaterialSystem?.Shutdown();
			mMaterialSystem = null;
			Materials.SetMaterialSystem( null );

			mPluginSystem?.Shutdown();
			mPluginSystem = null;
			Plugins.SetPluginSystem( null );

			mFileSystem?.Shutdown();
			mFileSystem = null;
			FileSystem.SetFileSystem( null );

			mConsole?.Shutdown();
			mConsole = null;
			Console.SetConsole( null );

			ShutdownReason = why;

			mHasShutdown = true;
			return false;
		}

		/// <summary>
		/// Updates the engine subsystems.
		/// </summary>
		/// <param name="delta">Delta time since last frame.</param>
		public void Update( float delta )
		{
			Console.Update( delta );

			foreach ( var app in Applications )
			{
				if ( !app.RunFrame( delta ) )
				{
					mPluginSystem.UnloadApplication( app );
				}
			}

			if ( Applications.Count == 0 )
			{
				Shutdown( "" );
			}
		}

		/// <summary>
		/// Renders everything for a given window.
		/// </summary>
		/// <param name="window">The window to render into.</param>
		public void RenderFrame( IWindow window )
		{
			IView? view = Render.Instance.GetView( window );
			if ( view is null )
			{
				mLogger.Error( "Cannot render frame - there is no renderview for the window!" );
				return;
			}

			RenderFrame( view );
		}

		/// <summary>
		/// Renders everything for a given view.
		/// </summary>
		/// <param name="view">The view to render from. Will render and update an <see cref="IWindow"/>.</param>
		public void RenderFrame( IView view )
		{
			Render.Instance.BeginFrame();
			Render.Instance.RenderView( view );
			Render.Instance.EndFrame();
			Render.Instance.PresentView( view );
		}

		private CoreInternal? mCore;
		private ConsoleInternal? mConsole;
		private FileSystemInternal? mFileSystem;
		private PluginSystemInternal? mPluginSystem;
		private MaterialSystemInternal? mMaterialSystem;

		private IWindowPlatform? mWindowPlatform;
		private IRenderFrontend? mRenderFrontend;
		private IView? mRenderView;

		private string[] mCommandlineArgs;
		private bool mHasShutdown = false;

		EngineConfig mEngineConfig;

		private IReadOnlyCollection<IApplication> Applications => mPluginSystem.ApplicationPlugins;
	}
}
