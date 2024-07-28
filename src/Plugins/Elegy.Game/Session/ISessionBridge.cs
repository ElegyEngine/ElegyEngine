// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Shared;

namespace Game.Session
{
	/// <summary>
	/// Handles communication between the server's world and
	/// the client's snapshot of the same.
	/// </summary>
	public interface ISessionBridge
	{
		/// <summary>
		/// In singleplayer: does nothing really.
		/// In multiplayer: receives gamestate packets from the server.
		/// </summary>
		void Update( float delta );

		/// <summary>
		/// Returns a list of entity states. In singleplayer,
		/// it returns a direct list of entities from the server.
		/// In multiplayer, it maintains its own list and deserialises
		/// from the server's gamestate packets.
		/// </summary>
		List<Entity> GetEntities();
	}
}
