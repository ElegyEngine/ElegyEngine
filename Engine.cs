// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy
{
	internal class Engine
	{
		public static Engine Instance { get; private set; }
		public static Node3D RootNode => Instance.mRootNode;

		public Engine( Node3D rootNode, string[] args )
		{
			mCommandlineArgs = args;
			mRootNode = rootNode;
			mEngineHostNode = (Node3D)mRootNode.FindChild( "EngineHost" );
			Instance = this;
		}

		public bool Init()
		{
			mHasShutdown = false;

			mConsole = new( mCommandlineArgs );
			mConsole.Init();

			Console.Log( "[Engine] Init" );
			Console.Warning( "[Engine] This is an early in-development build of the engine. DO NOT use in production!" );
			Console.Log( $"[Engine] Working directory: '{Directory.GetCurrentDirectory()}'", ConsoleMessageType.Verbose );

			if ( !LoadOrCreateEngineConfig( "engineConfig.json" ) )
			{
				return false;
			}

			if ( mEngineConfig.ConfigName != null )
			{
				Console.Log( $"[Engine] Engine config: '{mEngineConfig.ConfigName}'", ConsoleMessageType.Developer );
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

			Console.Log( "[Engine] Successfully initialised all systems" );
			mInitialisedSuccessfully = true;
			return true;
		}

		private bool LoadOrCreateEngineConfig( string path )
		{
			if ( !File.Exists( path ) )
			{
				Console.Log( $"[Engine] '{path}' does not exist, creating a default one..." );

				mEngineConfig = new();
				Text.JsonHelpers.Write( mEngineConfig, path );
				return true;
			}

			if ( !Text.JsonHelpers.LoadFrom( ref mEngineConfig, path ) )
			{
				Console.Error( $"[Engine] '{path}' somehow failed to load" );
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
				Console.Log( "[Engine] Shutting down normally..." );
			}
			else
			{
				Console.Error( $"[Engine] Shutting down, reason: {why}" );
			}

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
			mInitialisedSuccessfully = false;
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

		}

		public void HandleInput( InputEvent @event )
		{

		}

		private ConsoleInternal mConsole;
		private FileSystemInternal mFileSystem;
		private PluginSystemInternal mPluginSystem;

		private string[] mCommandlineArgs;
		private Node3D mRootNode;
		private Node3D mEngineHostNode;
		private bool mInitialisedSuccessfully = false;
		private bool mHasShutdown = false;

		EngineConfig mEngineConfig;

		private IReadOnlyCollection<IApplication> Applications => mPluginSystem.ApplicationPlugins;
	}
}
