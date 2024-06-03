// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.InputSystem.API;
using Elegy.FileSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderWorld;

using Silk.NET.Input;
using TestGame.Client;

using Elegy.PlatformSystem.API;
using Elegy.Common.Maths;
using Elegy.RenderSystem.Objects;

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

			mRenderWorld = new();
			Render.OnRender += mRenderWorld.RenderFrame;

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

		float roll = 0.0f;
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
				mClient.UpdateMovement( delta );

				if ( mRenderView is not null )
				{
					var state = mClient.Controller.GenerateControllerState();
					if ( mClient.Commands.ActionStates.HasFlag( ClientActions.LeanLeft ) )
					{
						roll += delta * 15.0f;
					}
					if ( mClient.Commands.ActionStates.HasFlag( ClientActions.LeanRight ) )
					{
						roll -= delta * 15.0f;
					}
					state.Angles.Z = roll;

					Coords.DirectionsFromDegrees( state.Angles, out var forward, out var up );
					mRenderView.Transform = Coords.CreateViewMatrix( state.Position, forward, up );

					Console.Log( $"XYZ:      {state.Position.X:F1} {state.Position.Y:F1} {state.Position.Z:F1}" );
					Console.Log( $"PitchYaw: {state.Angles.X:F1}° {state.Angles.Y:F1}°" );
					Console.Log( $"Forward:  {forward.X:F1} {forward.Y:F1} {forward.Z:F1}" );
					Console.Log( $"Up:       {up.X:F1} {up.Y:F1} {up.Z:F1}" );
				}
			}

			return !mUserWantsToExit;
		}

		private bool SpawnModel( string path, Vector3 position )
		{
			var renderMesh = Render.LoadMesh( path );
			if ( renderMesh is null )
			{
				mLogger.Error( $"Could not create rendermesh for '{path}'" );
				return false;
			}

			mRenderWorld.CreateEntity( false, renderMesh, position, Vector3.Zero );
			return true;
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

			SpawnModel( "models/oricube.glb", Vector3.Zero );
			SpawnModel( "models/terrain.glb", Vector3.Zero );

			mRenderView = Render.GetView( Platform.GetCurrentWindow() );
			mRenderView.Projection = Coords.CreatePerspectiveMatrix( MathF.PI / 2.0f, 16.0f / 9.0f, 0.01f, 4096.0f );
			mRenderView.Transform = Matrix4x4.CreateLookAt( new( 1.5f, 3.0f, 1.5f ), Vector3.Zero, Vector3.UnitZ );

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

		private RenderWorld mRenderWorld;
		private GameClient? mClient;
		private MainMenu mMenu = new();
		private View? mRenderView = null;

		private bool mGameIsLoaded = false;
		private bool mEscapeWasHeld = false;
		private bool mUserWantsToExit = false;
	}
}
