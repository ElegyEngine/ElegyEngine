// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine;
using Elegy.Engine.API;
using Elegy.Engine.Interfaces;

using Silk.NET.Input;
using TestGame.Client;

namespace TestGame
{
	public class Game : IApplication
	{
		private TaggedLogger mLogger = new( "Game" );

		public string Name => "Elegy test game";
		public string Error { get; private set; } = string.Empty;
		public bool Initialised { get; private set; } = false;

		public Game()
		{

		}

		public bool Init()
		{
			mLogger.Log( "Init" );
			Initialised = true;

			ApplicationConfig gameConfig = FileSystem.CurrentConfig;

			mLogger.Log( $" Name:      {gameConfig.Title}" );
			mLogger.Log( $" Developer: {gameConfig.Developer}" );
			mLogger.Log( $" Publisher: {gameConfig.Publisher}" );
			mLogger.Log( $" Version:   {gameConfig.Version}" );

			return true;
		}

		public bool Start()
		{
			mLogger.Log( "Start" );

			mMenu.OnNewGame = ( string mapName ) =>
			{
				StartGame( /*mapName*/ );
				mMenu.Visible = false;
			};

			mMenu.OnLeaveGame = () =>
			{
				LeaveGame();
			};

			mMenu.OnExit = () =>
			{
				LeaveGame();
				mUserWantsToExit = true;
			};

			mMenu.Init();

			return true;
		}

		public void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			mClient = null;
		}

		private void ToggleMenu()
		{
			if ( !mGameIsLoaded )
			{
				mMenu.Visible = true;
				Input.Mouse.Cursor.CursorMode = CursorMode.Normal;
				return;
			}

			mMenu.Visible = !mMenu.Visible;
			Input.Mouse.Cursor.CursorMode = mMenu.Visible ?
				CursorMode.Normal : CursorMode.Hidden;
		}

		public bool RunFrame( float delta )
		{
			// Quick little toggle quickly cobbled together,
			// until we have an extension to the input system
			if ( Input.Keyboard.IsKeyPressed( Key.Escape ) )
			{
				if ( !mEscapeWasHeld )
				{
					ToggleMenu();
				}
				mEscapeWasHeld = true;
			}
			else
			{
				mEscapeWasHeld = false;
			}

			if ( mGameIsLoaded )
			{
				mClient.Update();
				mClient.UpdateController();
			}

			return !mUserWantsToExit;
		}

		[ConsoleCommand( "map" )]
		public void StartGame()
		{
			if ( mGameIsLoaded )
			{
				LeaveGame();
			}

			mClient = new()
			{
				Controller = new BasicController()
			};

			mShowcaseModel = Assets.LoadModel( "models/test.gltf" );
			if ( mShowcaseModel is null )
			{
				mLogger.Error( "Cannot load 'models/test.gltf'" );
			}

			mLogger.Success( "Map successfully loaded, enjoy" );
			mGameIsLoaded = true;

			mMenu.Visible = false;
			mMenu.InGame = true;
		}

		[ConsoleCommand( "disconnect" )]
		public void LeaveGame()
		{
			if ( !mGameIsLoaded )
			{
				return;
			}

			mLogger.Log( "Leaving the game..." );

			mClient = null;

			mGameIsLoaded = false;
			mMenu.Visible = true;
			mMenu.InGame = false;
		}

		[ConsoleCommand( "exit" )]
		public void Exit()
		{
			LeaveGame();
			mUserWantsToExit = true;
		}

		#region Move elsewhere

		// TODO: manage entities elsewhere
		// PROPER ENTITY SYSTEM REQUIRED
		public T CreateEntity<T>() where T : Entities.Entity, new()
		{
			T entity = new();
			entity.Spawn();
			mEntities.Add( entity );

			return entity;
		}

		public T? FindEntity<T>() where T : Entities.Entity
		{
			foreach ( var entity in mEntities )
			{
				if ( entity is T )
				{
					return entity as T;
				}
			}

			return null;
		}

		#endregion
		private GameClient? mClient;
		private MainMenu mMenu = new();
		private Model? mShowcaseModel = null;
		
		private bool mGameIsLoaded = false;
		private bool mEscapeWasHeld = false;
		private bool mUserWantsToExit = false;
	}
}
