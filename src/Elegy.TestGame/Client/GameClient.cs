// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Extensions;
using Elegy.Engine.API;

using Silk.NET.Input;

namespace TestGame.Client
{
	public class GameClient
	{
		public GameClient()
		{
			GrabMouse();
		}

		public void Update()
		{
			//Basis basis = new Basis( Quaternion.FromEuler( mAngles * new Vector3( 0.0f, 1.0f, 0.0f ) ) );
			Vector3 viewForward = Vector3.UnitY; // -basis.Z;
			Vector3 viewRight = Vector3.UnitX; //basis.X;

			mMovementDirection = Vector3.Zero;

			if ( Input.Keyboard.IsKeyPressed( Key.W ) )
			{
				mMovementDirection += viewForward;
			}
			if ( Input.Keyboard.IsKeyPressed( Key.A ) )
			{
				mMovementDirection -= viewRight;
			}
			if ( Input.Keyboard.IsKeyPressed( Key.S ) )
			{
				mMovementDirection -= viewForward;
			}
			if ( Input.Keyboard.IsKeyPressed( Key.D ) )
			{
				mMovementDirection += viewRight;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.Space ) )
			{
				mMovementDirection += Vector3.UnitZ;
			}
			if ( Input.Keyboard.IsKeyPressed( Key.ControlLeft ) )
			{
				mMovementDirection -= Vector3.UnitZ;
			}

			mMousePositionDeltaSmooth = mMousePositionDeltaSmooth.Lerp( mMousePositionDelta, 0.99f );
			//mMousePositionDeltaSmooth = mMousePositionDelta;

			mAngles.Y -= mMousePositionDeltaSmooth.X * 0.001f;
			mAngles.X -= mMousePositionDeltaSmooth.Y * 0.001f;

			mCommands.MovementDirection = mMovementDirection;
			mCommands.ViewAngles = mAngles;
			mCommands.ActionStates = GrabActionStates();

			var state = Controller.GenerateControllerState();
			mPresentation.Position = state.Position;
			mPresentation.Angles = mAngles;
			mPresentation.Update();

			mMousePositionDelta = Vector2.Zero;
		}

		public void UpdateController()
		{
			Controller.HandleClientInput( Commands );
		}

		public void GrabMouse()
		{
			Input.Mouse.Cursor.CursorMode = CursorMode.Hidden;
		}

		public void ReleaseMouse()
		{
			Input.Mouse.Cursor.CursorMode = CursorMode.Normal;
		}

		private ClientActions GrabActionStates()
		{
			ClientActions actionStates = 0;

			if ( Input.Mouse.IsButtonPressed( MouseButton.Left ) )
				actionStates |= ClientActions.PrimaryAttack;
			if ( Input.Mouse.IsButtonPressed( MouseButton.Right ) )
				actionStates |= ClientActions.SecondaryAttack;
			if ( Input.Mouse.IsButtonPressed( MouseButton.Middle ) )
				actionStates |= ClientActions.TertiaryAttack;

			if ( Input.Keyboard.IsKeyPressed( Key.ShiftLeft ) )
				actionStates |= ClientActions.Sprint;
			if ( Input.Keyboard.IsKeyPressed( Key.E ) )
				actionStates |= ClientActions.Use;
			if ( Input.Keyboard.IsKeyPressed( Key.R ) )
				actionStates |= ClientActions.Reload;
			if ( Input.Keyboard.IsKeyPressed( Key.F ) )
				actionStates |= ClientActions.Flashlight;

			if ( Input.Keyboard.IsKeyPressed( Key.AltLeft ) )
			{
				if ( Input.Keyboard.IsKeyPressed( Key.A ) )
					actionStates |= ClientActions.LeanLeft;
				if ( Input.Keyboard.IsKeyPressed( Key.D ) )
					actionStates |= ClientActions.LeanRight;
				if ( Input.Keyboard.IsKeyPressed( Key.W ) )
					actionStates |= ClientActions.LeanForward;
			}

			if ( Input.Keyboard.IsKeyPressed( Key.Escape ) )
				actionStates |= ClientActions.Escape;
			if ( Input.Keyboard.IsKeyPressed( Key.Tab ) )
				actionStates |= ClientActions.Tab;
			if ( Input.Keyboard.IsKeyPressed( Key.Enter ) )
				actionStates |= ClientActions.Enter;

			return actionStates;
		}

		public IPlayerControllable Controller { get; set; }
		public ClientCommands Commands { get => mCommands; }

		private ClientCommands mCommands = new();

		Presentation mPresentation = new();

		private Vector2 mMousePositionDelta = Vector2.Zero;
		private Vector2 mMousePositionDeltaSmooth = Vector2.Zero;

		private Vector3 mMovementDirection = Vector3.Zero;
		private Vector3 mAngles = Vector3.Zero;
	}
}
