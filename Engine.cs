// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

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
			Console.SetConsole( mConsole );
			mConsole.Init();

			Console.Log( "Working directory:" );
			Console.Log( Directory.GetCurrentDirectory() );

			mPluginSystem = new( "engineConfig.json" );
			Plugins.SetPluginSystem( mPluginSystem );
			if ( !mPluginSystem.Init() )
			{
				return Shutdown( "Plugin system failure", true );
			}

			Console.Log( "Successfully initialised Elegy Engine" );
			mInitialisedSuccessfully = true;
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
				Console.Log( "Shutting down normally..." );
			}
			else
			{
				Console.Error( $"Shutting down, reason: {why}" );
			}

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

		private Internal.Console mConsole;
		private PluginSystem mPluginSystem;

		private string[] mCommandlineArgs;
		private Node3D mRootNode;
		private Node3D mEngineHostNode;
		private bool mInitialisedSuccessfully = false;
		private bool mHasShutdown = false;

		private IReadOnlyCollection<IApplication> Applications => mPluginSystem.ApplicationPlugins;
	}
}
