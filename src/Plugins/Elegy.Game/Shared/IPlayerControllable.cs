// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared
{
	// What the server sends back to the client
	public struct PlayerControllerState
	{
		public Vector3 Position;
		public Vector3 Angles; // mmm not actually needed but oh well
	}

	public interface IPlayerControllable
	{
		void Update( float delta );
		void HandleClientInput( ClientCommands commands );
		PlayerControllerState GenerateControllerState();
	}
}
