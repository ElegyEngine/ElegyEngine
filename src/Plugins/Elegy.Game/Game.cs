﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.Common.Maths;
using Elegy.ConsoleSystem;
using Elegy.InputSystem.API;
using Elegy.FileSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Elegy.RenderWorld;

using Game.Client;

using Silk.NET.Input;
using System.Diagnostics;
using Game.Shared;

namespace Game
{
	public class Game : IApplication
	{
		private TaggedLogger mLogger = new( "Game" );
		private Stopwatch mStopwatch = new();

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
			mStopwatch.Restart();

			mMenu.OnNewGame = ( string mapName ) =>
			{
				StartGame( mapName );
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
			StartGame( "maps/test2" );

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

					//Console.Log( $"XYZ:      {state.Position.X:F1} {state.Position.Y:F1} {state.Position.Z:F1}" );
					//Console.Log( $"PitchYaw: {state.Angles.X:F1}° {state.Angles.Y:F1}°" );
					//Console.Log( $"Forward:  {forward.X:F1} {forward.Y:F1} {forward.Z:F1}" );
					//Console.Log( $"Up:       {up.X:F1} {up.Y:F1} {up.Z:F1}" );
				}

				if ( mOricubeRenderEntity != -1 )
				{
					var renderEntity = mRenderWorld.MeshEntities[mOricubeRenderEntity];
					float time = mStopwatch.ElapsedMilliseconds * 0.001f;

					renderEntity.Transform = Coords.CreateWorldMatrixRadians(
						// Bob up'n'down slightly
						position: Coords.Up * 0.2f * MathF.Sin( time * 2.5f ),
						// 1/24th of a turn is like 15 degrees
						angles: Coords.TurnUp * (1.0f / 24.0f) * MathF.Sin( time ) );
				}
			}

			return !mUserWantsToExit;
		}

		private bool LoadLevel( string path )
		{
			string pathWithExt = Path.ChangeExtension( path, ".elf" );
			string? fullPath = Files.PathTo( pathWithExt, PathFlags.File );

			if ( fullPath is null )
			{
				mLogger.Error( $"Level '{pathWithExt}' does not exist." );

				// Check for a TrenchBroom .map file, chances are the user did not compile
				// the .elf for whatever reason.
				string originalMapPath = Path.ChangeExtension( path, ".map" );
				fullPath = Files.PathTo( originalMapPath, PathFlags.File );
				if ( fullPath is not null )
				{
					// TODO: some people probably won't be reading the compile log, so what if we
					// implemented intelligent log reading? The formatting is super consistent and
					// it should be easy to find errors
					mLogger.Log( "There's a .map file with the same name though. Did you compile it?" );
					mLogger.Log( "If you did and it's not there, please read the compile log." );
				}

				return false;
			}

			ElegyMapDocument? map = Assets.LoadLevel( fullPath );
			if ( map is null )
			{
				mLogger.Error( "Error while reading the map file" );
				return false;
			}



			return true;
		}

		private int SpawnModel( string path, Vector3 position )
		{
			var renderMesh = Render.LoadMesh( path );
			if ( renderMesh is null )
			{
				mLogger.Error( $"Could not create rendermesh for '{path}'" );
				return -1;
			}

			return mRenderWorld.CreateEntity( false, renderMesh, position, Vector3.Zero );
		}

		private int mOricubeRenderEntity = -1;
		private int mTerrainRenderEntity = -1;

		[ConsoleCommand( "map" )]
		public void StartGame( string path )
		{
			if ( mGameIsLoaded )
			{
				LeaveGame();
			}

			mLogger.Log( $"Loading level '{path}'..." );
			if ( !LoadLevel( path ) )
			{
				mLogger.Error( $"Could not load level '{path}'" );
				return;
			}

			mClient = new()
			{
				Controller = new BasicController()
			};

			mOricubeRenderEntity = SpawnModel( "models/oricube.glb", Coords.Forward * 5.0f );
			mTerrainRenderEntity = SpawnModel( "models/terrain.glb", Vector3.Zero );

			mRenderView = Render.GetCurrentWindowView();
			if ( mRenderView is not null )
			{
				mRenderView.Projection = Coords.CreatePerspectiveMatrix( MathF.PI / 2.0f, 16.0f / 9.0f, 0.01f, 4096.0f );
				mRenderView.Transform = Matrix4x4.CreateLookAt( new( 1.5f, 3.0f, 1.5f ), Vector3.Zero, Vector3.UnitZ );
			}

			var textureSamplerId = Render.GetGlobalParameterIndex( "Sampler" );
			Render.SetGlobalParameter( textureSamplerId, Render.Samplers.Nearest );

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
