// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared
{
	/// <summary>
	/// Movement data the server sends back to the client.
	/// </summary>
	public struct PlayerControllerState
	{
		public Vector3 Position;
		public Vector3 Angles; // mmm not actually needed but oh well
	}

	/// <summary>
	/// An entity that is controllable by the player. This is not meant
	/// to be implemented by components! Rather, components can refer
	/// to a controller object.
	/// </summary>
	public interface IPlayerControllable
	{
		void Update( float delta );
		void HandleClientInput( ClientCommands commands );
		PlayerControllerState GenerateControllerState();
	}
}
