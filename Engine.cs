// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public class Engine
	{
		public Engine( Node3D rootNode, string[] args )
		{
			mRootNode = rootNode;
			mEngineHostNode = (Node3D)mRootNode.FindChild( "EngineHost" );
		}

		public bool Init()
		{
			mHasShutdown = false;

			GD.Print( "Successfully initialised Elegy Engine" );
			mInitialisedSuccessfully = true;
			return true;
		}

		public bool Shutdown( string why = "", bool hardExit = false )
		{
			if ( mHasShutdown )
			{
				return true;
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

		}

		public void PhysicsUpdate( float delta )
		{

		}

		public void HandleInput( InputEvent @event )
		{

		}

		private Node3D mRootNode;
		private Node3D mEngineHostNode;
		private bool mInitialisedSuccessfully = false;
		private bool mHasShutdown = false;
	}
}
