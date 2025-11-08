// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared
{
	public class FlyCamController : IClientController
	{
		Vector3 mPosition;
		Vector3 mViewAngles;

		public void Update( float dt, ClientCommand command )
		{
			mPosition += command.MovementDirection * dt;
			mViewAngles = command.ViewAngles;
		}

		public PlayerControllerState GenerateControllerState()
			=> new()
			{
				Position = mPosition,
				Angles = mViewAngles
			};
	}
}
