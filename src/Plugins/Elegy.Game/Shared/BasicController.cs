﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared
{
	public class BasicController : IPlayerControllable
	{
		Vector3 mPosition = new();
		Vector3 mDirection = new();
		Vector3 mViewAngles = new();

		public void Setup( EntityWorld world )
		{

		}

		public void Update( float dt )
		{
			mPosition += mDirection * dt;
		}

		public PlayerControllerState GenerateControllerState()
			=> new()
			{
				Position = mPosition,
				Angles = mViewAngles
			};

		public void HandleClientInput( ClientCommands commands )
		{
			mViewAngles = commands.ViewAngles;
			mDirection = commands.MovementDirection;
		}
	}
}
