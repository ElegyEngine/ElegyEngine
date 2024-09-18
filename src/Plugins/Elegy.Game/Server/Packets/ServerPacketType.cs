// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Server.Packets
{
	/// <summary>
	/// Types of packets that the server can send to a client.
	/// </summary>
	public enum ServerPacketType
	{
		/// <summary>
		/// Forcibly disconnects a client, sends a short string to the
		/// client with a reason for the disconnection. Typically happens
		/// whenever the server shuts down, or when a client is kicked/banned etc.
		/// </summary>
		Disconnect,

		/// <summary>
		/// Authentication & authorisation request.
		/// Optionally asks for a password too.
		/// </summary>
		AuthRequest,

		/// <summary>
		/// Sends information about all used assets by the server, meaning
		/// the level file, models, materials and so on.
		/// </summary>
		AssetInfoRequest,

		/// <summary>
		/// Can be URLs to Steam workshop items, or actual binary asset data.
		/// </summary>
		AssetPayload,

		/// <summary>
		/// Sends a full snapshot of the game state (can come in chunks),
		/// and an entity ID.
		/// </summary>
		SpawnResponse,

		/// <summary>
		/// Acknowledgment to <see cref="Session.Packets.ClientPacketType.SpawnComplete"/>.
		/// </summary>
		SpawnCompleteAck,

		/// <summary>
		/// Contains entity state data.
		/// </summary>
		GameStatePayload,

		/// <summary>
		/// Contains game event/RPC data.
		/// </summary>
		GameEventPayload
	}
}
