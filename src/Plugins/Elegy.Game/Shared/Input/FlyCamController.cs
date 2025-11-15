// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared.Input
{
	public class FlyCamController : IClientController
	{
		Vector3 mPosition;

		public PlayerControllerState Update( float dt, ClientCommand command )
		{
			mPosition += command.MovementDirection * dt;

			return new()
			{
				Position = mPosition,
				Angles = command.ViewAngles
			};
		}
	}
}
