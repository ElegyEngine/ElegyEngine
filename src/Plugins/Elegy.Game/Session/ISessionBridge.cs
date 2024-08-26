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
		/// 
		/// It's important to note that this list doesn't
		/// get re-generated. It is instead maintained.
		/// </summary>
		List<Entity> GetEntities();

		/// <summary>
		/// Returns an asset registry for asset string bookkeeping.
		/// In singleplayer, it returns the server's own copy directly.
		/// In multiplayer, it constructs itself from the server's packets.
		/// </summary>
		AssetRegistry GetAssetRegistry();

		/// <summary>
		/// Sends a <see cref="ClientCommands"/> snapshot to the server.
		/// This is performed by clients multiple times per second.
		/// 
		/// In singleplayer, this directly copies <paramref name="snapshot"/>
		/// into the server's client snapshot buffer.
		/// In multiplayer, it emits it as a packet.
		/// </summary>
		void SendInputSnapshot( in ClientCommands snapshot );
	}
}
