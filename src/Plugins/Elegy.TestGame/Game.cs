// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.InputSystem.API;
using Elegy.FileSystem.API;
using Elegy.RenderSystem.API;

using Silk.NET.Input;
using TestGame.Client;

using IRenderView = Elegy.RenderSystem.Interfaces.Rendering.IView;
using IRenderEntity = Elegy.RenderSystem.Interfaces.Rendering.IEntity;
using Elegy.PlatformSystem.API;
using Elegy.Common.Maths;

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

			ApplicationConfig gameConfig = Files.CurrentConfig;

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

			StartGame();

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

			mShowcaseModel = Assets.LoadModel( "models/test.glb" );
			if ( mShowcaseModel is null )
			{
				mLogger.Error( "Cannot load 'models/test.glb'" );
			}
			else
			{
				mShowcaseEntity = Render.Instance.CreateEntity( false );
				mShowcaseEntity.Mesh = Render.Instance.CreateMesh( mShowcaseModel );
			}

			mRenderView = Render.Instance.GetView( Platform.GetCurrentWindow() );
			mRenderView.Projection = Matrix4x4.CreatePerspectiveFieldOfView( MathF.PI / 4.0f, 16.0f / 9.0f, 0.01f, 4096.0f );
			mRenderView.Transform = Matrix4x4.CreateLookAt( new( 10.0f, 20.0f, 10.0f ), Vector3.Zero, Vector3.UnitZ );

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

		private GameClient? mClient;
		private MainMenu mMenu = new();
		private Model? mShowcaseModel = null;
		private IRenderEntity? mShowcaseEntity = null;
		private IRenderView? mRenderView = null;
		
		private bool mGameIsLoaded = false;
		private bool mEscapeWasHeld = false;
		private bool mUserWantsToExit = false;
	}
}
