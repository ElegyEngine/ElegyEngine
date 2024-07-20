// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Client
{
	// What the server sends back to the client
	public struct PlayerControllerState
	{
		public Vector3 Position;
		public Vector3 Angles; // mmm not actually needed but oh well
	}

	public interface IPlayerControllable
	{
		void HandleClientInput( ClientCommands commands );
		PlayerControllerState GenerateControllerState();
	}
}
