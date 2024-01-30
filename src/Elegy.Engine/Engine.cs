// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using Silk.NET.Windowing;
using System.Diagnostics;

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

			mLogger.Log( "Successfully initialised all systems" );
			return true;
		}

		/// <summary>
		/// Kicks off the update loop.
		/// </summary>
		public void Run()
		{
			Stopwatch sw = Stopwatch.StartNew();
			double lastTime = -0.016;

			while ( !mHasShutdown )
			{
				double currentTime = (double)sw.ElapsedTicks / Stopwatch.Frequency;
				Update( (float)(currentTime - lastTime) );
				lastTime = currentTime;
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
		private void Update( float delta )
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

		private CoreInternal? mCore;
		private ConsoleInternal? mConsole;
		private FileSystemInternal? mFileSystem;
		private PluginSystemInternal? mPluginSystem;
		private MaterialSystemInternal? mMaterialSystem;

		private IWindowPlatform? mWindowPlatform;
		
		private string[] mCommandlineArgs;
		private bool mHasShutdown = false;

		EngineConfig mEngineConfig;

		private IReadOnlyCollection<IApplication> Applications => mPluginSystem.ApplicationPlugins;
	}
}
