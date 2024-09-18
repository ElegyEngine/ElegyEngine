// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Session.Packets
{
	/// <summary>
	/// Types of packets that the client can send to a server.
	/// </summary>
	public enum ClientPacketType
	{
		/// <summary>
		/// Requests to join a server.
		/// </summary>
		JoinRequest,

		/// <summary>
		/// Authentication & authorisation response.
		/// Optionally provides a password.
		/// </summary>
		AuthResponse,

		/// <summary>
		/// Sends information about missing assets and hashes of all precached assets.
		/// </summary>
		AssetInfoResponse,

		/// <summary>
		/// Requests the server to concretely spawn a player entity,
		/// so that it can finish joining.
		/// </summary>
		SpawnRequest,

		/// <summary>
		/// The client has set up everything and is ready to send input snapshots,
		/// and receive game state packets.
		/// </summary>
		SpawnComplete,

		/// <summary>
		/// Contains entity state data.
		/// </summary>
		InputPayload,

		/// <summary>
		/// Contains game event/RPC data.
		/// </summary>
		EventPayload
	}
}
