﻿// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy
{
	internal class Engine
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

		public static readonly string VersionString = $"{MajorVersion}.{MinorVersion}";

		public static Engine Instance { get; private set; }
		public static Node3D RootNode => Instance.mRootNode;

		public static DateTime StartupTime { get; private set; }

		public const string Tag = "Engine";

		#region Console commands
		[ConsoleCommand( "test" )]
		public static bool Command_Test( int a, int b = 20 )
		{
			Console.Log( $"You've successfully called 'test' with {a} and {b}!" );
			return true;
		}

		[ConsoleCommand( "test_badparams" )]
		public static bool Command_BadParameters( byte a, short b, long c, Half d, DateTime e )
		{
			return true;
		}

		[ConsoleCommand( "test_badreturn" )]
		public static int Command_BadReturnType( int a )
		{
			return 0;
		}

		[ConsoleCommand( "test_args" )]
		public static void Command_OnlyArgs( string[] args )
		{

		}

		[ConsoleCommand( "test_noparams" )]
		public static void Command_NoParameters()
		{

		}

		[ConsoleCommand( "test_nonstatic" )]
		public void Command_NonStatic()
		{

		}
		#endregion
		public Engine( Node3D rootNode, string[] args )
		{
			mCommandlineArgs = args;
			mRootNode = rootNode;
			mEngineHostNode = (Node3D)mRootNode.FindChild( "EngineHost" );
			StartupTime = DateTime.Now;
			Instance = this;
		}

		public bool Init()
		{
			mHasShutdown = false;

			if ( !InitialiseConsole() )
			{
				return Shutdown( "Console system failure", true );
			}

			if ( !LoadOrCreateEngineConfig( "engineConfig.json" ) )
			{
				return Shutdown( "Configuration failure", true );
			}

			if ( mEngineConfig.ConfigName != null )
			{
				Console.Log( Tag, $"Engine config: '{mEngineConfig.ConfigName}'", ConsoleMessageType.Developer );
			}

			mFileSystem = new( mEngineConfig );
			if ( !mFileSystem.Init() )
			{
				return Shutdown( "File system failure", true );
			}

			mPluginSystem = new();
			if ( !mPluginSystem.Init() )
			{
				return Shutdown( "Plugin system failure", true );
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
				return Shutdown( "Material system failure", true );
			}

			Console.Log( Tag, "Successfully initialised all systems" );
			return true;
		}

		private bool InitialiseConsole()
		{
			mConsole = new( mCommandlineArgs );
			if ( !mConsole.Init() )
			{
				return false;
			}

			Console.Log( $"Initialising Elegy Engine ({VersionString}) by Admer456" );

			if ( MajorVersion < 1 )
			{
				Console.Warning( Tag, "This is an early in-development build of the engine. DO NOT use in production!" );
			}

			Console.Log( Tag, $"Working directory: '{Directory.GetCurrentDirectory()}'", ConsoleMessageType.Verbose );
			return true;
		}

		private bool LoadOrCreateEngineConfig( string path )
		{
			if ( !File.Exists( path ) )
			{
				Console.Log( Tag, $"'{path}' does not exist, creating a default one..." );

				mEngineConfig = new();
				Text.JsonHelpers.Write( mEngineConfig, path );
				return true;
			}

			if ( !Text.JsonHelpers.LoadFrom( ref mEngineConfig, path ) )
			{
				Console.Error( Tag, $"'{path}' somehow failed to load" );
				return false;
			}

			return true;
		}

		public bool Shutdown( string why = "", bool hardExit = false )
		{
			if ( mHasShutdown )
			{
				return true;
			}

			if ( why == "" )
			{
				Console.Log( Tag, "Shutting down normally..." );
			}
			else
			{
				Console.Error( Tag, $"Shutting down, reason: {why}" );
			}

			mMaterialSystem.Shutdown();
			mMaterialSystem = null;
			Materials.SetMaterialSystem( null );

			mPluginSystem.Shutdown();
			mPluginSystem = null;
			Plugins.SetPluginSystem( null );

			mFileSystem.Shutdown();
			mFileSystem = null;
			FileSystem.SetFileSystem( null );

			mConsole.Shutdown();
			mConsole = null;
			Console.SetConsole( null );

			if ( hardExit )
			{
				ExitEngine( why == "" ? 0 : 99 );
			}

			mHasShutdown = true;
			return false;
		}

		/// <summary>
		/// Actually quits Godot
		/// </summary>
		private void ExitEngine( int returnCode )
		{
			mRootNode.GetTree().Quit( returnCode );
			mRootNode.QueueFree();
		}

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
				Shutdown( "", true );
			}
		}

		public void PhysicsUpdate( float delta )
		{
			foreach ( var app in Applications )
			{
				app.RunPhysicsFrame( delta );
			}
		}

		public void HandleInput( InputEvent @event )
		{
			foreach ( var app in Applications )
			{
				app.HandleInput( @event );
			}
		}

		private ConsoleInternal? mConsole;
		private FileSystemInternal? mFileSystem;
		private PluginSystemInternal? mPluginSystem;
		private MaterialSystemInternal? mMaterialSystem;

		private string[] mCommandlineArgs;
		private Node3D mRootNode;
		private Node3D mEngineHostNode;
		private bool mHasShutdown = false;

		EngineConfig mEngineConfig;

		private IReadOnlyCollection<IApplication> Applications => mPluginSystem.ApplicationPlugins;
	}
}