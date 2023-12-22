// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Client
{
	public class Client
	{
		public Client()
		{
			GrabMouse();
		}

		public void UserInput( InputEvent @event )
		{
			if ( @event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured )
			{
				var mouseMotion = @event as InputEventMouseMotion;
				mMousePositionDelta += mouseMotion.Relative;
			}
		}

		public void Update()
		{
			Basis basis = new Basis( Quaternion.FromEuler( mAngles * new Vector3( 0.0f, 1.0f, 0.0f ) ) );
			Vector3 viewForward = -basis.Z;
			Vector3 viewRight = basis.X;

			mMovementDirection = Vector3.Zero;

			if ( Input.IsKeyPressed( Key.W ) )
			{
				mMovementDirection += viewForward;
			}
			if ( Input.IsKeyPressed( Key.A ) )
			{
				mMovementDirection -= viewRight;
			}
			if ( Input.IsKeyPressed( Key.S ) )
			{
				mMovementDirection -= viewForward;
			}
			if ( Input.IsKeyPressed( Key.D ) )
			{
				mMovementDirection += viewRight;
			}

			if ( Input.IsKeyPressed( Key.Space ) )
			{
				mMovementDirection += Vector3.Up;
			}
			if ( Input.IsKeyPressed( Key.Ctrl ) )
			{
				mMovementDirection -= Vector3.Up;
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
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		public void ReleaseMouse()
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		private ClientActions GrabActionStates()
		{
			ClientActions actionStates = 0;

			if ( Input.IsMouseButtonPressed( MouseButton.Left ) )
				actionStates |= ClientActions.PrimaryAttack;
			if ( Input.IsMouseButtonPressed( MouseButton.Right ) )
				actionStates |= ClientActions.SecondaryAttack;
			if ( Input.IsMouseButtonPressed( MouseButton.Middle ) )
				actionStates |= ClientActions.TertiaryAttack;

			if ( Input.IsKeyPressed( Key.Shift ) )
				actionStates |= ClientActions.Sprint;
			if ( Input.IsKeyPressed( Key.E ) )
				actionStates |= ClientActions.Use;
			if ( Input.IsKeyPressed( Key.R ) )
				actionStates |= ClientActions.Reload;
			if ( Input.IsKeyPressed( Key.F ) )
				actionStates |= ClientActions.Flashlight;

			if ( Input.IsKeyPressed( Key.Alt ) )
			{
				if ( Input.IsKeyPressed( Key.A ) )
					actionStates |= ClientActions.LeanLeft;
				if ( Input.IsKeyPressed( Key.D ) )
					actionStates |= ClientActions.LeanRight;
				if ( Input.IsKeyPressed( Key.W ) )
					actionStates |= ClientActions.LeanForward;
			}

			if ( Input.IsKeyPressed( Key.Escape ) )
				actionStates |= ClientActions.Escape;
			if ( Input.IsKeyPressed( Key.Tab ) )
				actionStates |= ClientActions.Tab;
			if ( Input.IsKeyPressed( Key.Enter ) )
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
