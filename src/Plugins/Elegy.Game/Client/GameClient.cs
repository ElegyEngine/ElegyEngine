﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Elegy.Common.Maths;
using Elegy.ConsoleSystem;
using Elegy.InputSystem.API;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Game.Shared;
using Silk.NET.Input;

namespace Game.Client
{
	public class GameClient
	{
		private TaggedLogger mLogger = new( "Client" );
		private InputSystem mInput = new();
		private MainMenu mMenu = new();

		private ClientCommands mCommands;

		private Vector2 mOldMousePosition = Vector2.Zero;
		private Vector2 mMousePositionDelta = Vector2.Zero;
		private Vector2 mMousePositionDeltaSmooth = Vector2.Zero;

		private Vector3 mMovementDirection = Vector3.Zero;
		private Vector3 mAngles = Vector3.Zero;

		private View? mRenderView;

		public ClientCommands Commands => mCommands;
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
			
			Coords.DirectionsFromDegrees( mAngles, out var viewForward, out var viewUp );
			Vector3 viewRight = viewForward.Cross( viewUp );

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

			mMousePositionDelta = Input.Mouse.Position - mOldMousePosition;
			mMousePositionDeltaSmooth = mMousePositionDeltaSmooth.Lerp( mMousePositionDelta, 0.99f );
			//mMousePositionDeltaSmooth = mMousePositionDelta;

			mCommands.MovementDirection = mMovementDirection;
			mCommands.ActionStates = GrabActionStates();

			if ( mCommands.ActionStates.HasFlag( ClientActions.SecondaryAttack ) )
			{
				mAngles.Y += mMousePositionDeltaSmooth.X * 0.06f;
				mAngles.X -= mMousePositionDeltaSmooth.Y * 0.06f;
			}

			mCommands.ViewAngles = mAngles;
			mMousePositionDelta = Vector2.Zero;
			mOldMousePosition = Input.Mouse.Position;
		}

		// TODO: delegate to the input system
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

			if ( Input.Keyboard.IsKeyPressed( Key.Y ) )
				actionStates |= ClientActions.LeanLeft;
			if ( Input.Keyboard.IsKeyPressed( Key.C ) )
				actionStates |= ClientActions.LeanRight;

			if ( Input.Keyboard.IsKeyPressed( Key.AltLeft ) )
			{
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
	}
}
