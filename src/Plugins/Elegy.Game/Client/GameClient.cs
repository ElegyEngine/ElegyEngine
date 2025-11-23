// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.LogSystem;
using Elegy.InputSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Game.Shared;
using Game.Shared.Input;
using Game.Shared.Input.Actions;
using Silk.NET.Input;

namespace Game.Client
{
	public class GameClient
	{
		private TaggedLogger mLogger = new( "Client" );
		private InputSystem mInput = new();
		private MainMenu mMenu = new();

		private ClientCommand mCommand;

		private Vector2 mOldMousePosition = Vector2.Zero;
		private Vector2 mMousePositionDelta = Vector2.Zero;
		private Vector2 mMousePositionDeltaSmooth = Vector2.Zero;

		private Vector3 mMovementDirection = Vector3.Zero;
		private Vector3 mAngles = Vector3.Zero;

		private View? mRenderView;

		public ClientCommand Command => mCommand;
		public View RenderView => mRenderView;

		public bool Init()
		{
			mLogger.Log( "Init" );

			mRenderView = Render.GetCurrentWindowView();
			Debug.Assert( mRenderView is not null );

			if ( !mInput.Init() )
			{
				return false;
			}

			mMenu.Init();
			mMenu.Visible = true;

			return true;
		}

		public void Shutdown()
		{
			mLogger.Log( "Shutdown" );
			mInput.Shutdown();
		}

		public void Update( float delta )
		{
			mLogger.Log( $"Update: {delta * 1000.0f:F} ms" );
			mLogger.Log( $"Draw CPU: {Render.CpuFrametime * 1000.0f:F} ms" );
			mLogger.Log( $"Draw GPU: {Render.GpuFrametime * 1000.0f:F} ms" );

			mMovementDirection = Vector3.Zero;

			if ( Input.Keyboard.IsKeyPressed( Key.W ) )
			{
				mMovementDirection += Coords.Forward;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.A ) )
			{
				mMovementDirection += Coords.Left;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.S ) )
			{
				mMovementDirection += Coords.Back;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.D ) )
			{
				mMovementDirection += Coords.Right;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.Space ) )
			{
				mMovementDirection += Vector3.UnitZ;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.ControlLeft ) )
			{
				mMovementDirection -= Vector3.UnitZ;
			}

			mMousePositionDelta = Input.Mouse.Position - mOldMousePosition;
			mMousePositionDeltaSmooth = mMousePositionDeltaSmooth.Lerp( mMousePositionDelta, 0.99f );
			//mMousePositionDeltaSmooth = mMousePositionDelta;

			mCommand.MovementDirection = mMovementDirection;
			mCommand.ActionFlags = GrabActionStates();

			if ( mCommand.HasAction( PlayerActions.SecondaryAttack ) )
			{
				mAngles.Y += mMousePositionDeltaSmooth.X * 0.06f;
				mAngles.X -= mMousePositionDeltaSmooth.Y * 0.06f;
			}

			mCommand.ViewAngles = mAngles;
			mMousePositionDelta = Vector2.Zero;
			mOldMousePosition = Input.Mouse.Position;
		}

		// TODO: delegate to the input system
		private int GrabActionStates()
		{
			PlayerActions actionStates = 0;

			if ( Input.Mouse.IsButtonPressed( MouseButton.Left ) )
				actionStates |= PlayerActions.PrimaryAttack;
			if ( Input.Mouse.IsButtonPressed( MouseButton.Right ) )
				actionStates |= PlayerActions.SecondaryAttack;
			if ( Input.Mouse.IsButtonPressed( MouseButton.Middle ) )
				actionStates |= PlayerActions.TertiaryAttack;

			if ( Input.Keyboard.IsKeyPressed( Key.ShiftLeft ) )
				actionStates |= PlayerActions.Sprint;
			if ( Input.Keyboard.IsKeyPressed( Key.E ) )
				actionStates |= PlayerActions.Use;
			if ( Input.Keyboard.IsKeyPressed( Key.R ) )
				actionStates |= PlayerActions.Reload;
			if ( Input.Keyboard.IsKeyPressed( Key.F ) )
				actionStates |= PlayerActions.Flashlight;

			if ( Input.Keyboard.IsKeyPressed( Key.Y ) )
				actionStates |= PlayerActions.LeanLeft;
			if ( Input.Keyboard.IsKeyPressed( Key.C ) )
				actionStates |= PlayerActions.LeanRight;

			if ( Input.Keyboard.IsKeyPressed( Key.AltLeft ) )
			{
				if ( Input.Keyboard.IsKeyPressed( Key.W ) )
					actionStates |= PlayerActions.LeanForward;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.Escape ) )
				actionStates |= PlayerActions.Menu;
			if ( Input.Keyboard.IsKeyPressed( Key.Tab ) )
				actionStates |= PlayerActions.Tab;
			if ( Input.Keyboard.IsKeyPressed( Key.Enter ) )
				actionStates |= PlayerActions.Confirm;

			return (int)actionStates;
		}
	}
}
