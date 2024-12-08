// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Shared;

namespace Game.Server
{
	/// <summary>
	/// A bridge to communicate with clients. Local clients have
	/// a direct link while remote clients are communicated with
	/// through packets.
	/// </summary>
	public interface IClientBridge
	{
		/// <summary>
		/// Updates the connection.
		/// </summary>
		void Update( float delta );

		/// <summary>
		/// Local clients can bypass the whole authorisation step.
		/// </summary>
		bool Bypass { get; }

		#region Protocol
		/// <summary>
		/// Informs the client that it has been disconnected,
		/// e.g. in case of a kick/ban or the server shutting down.
		/// </summary>
		void SendDisconnect( string reason );

		/// <summary>
		/// Sends an authentication & authorisation request, optionally with
		/// a password. Maps to <see cref="Packets.ServerPacketType.AuthRequest"/>.
		/// </summary>
		void SendAuthRequest( bool passwordNeeded );

		/// <summary>
		/// Informs the client of all necessary assets that
		/// the server is currently working with.
		/// </summary>
		void SendAssetInfoRequest( AssetRegistry registry );

		/// <summary>
		/// Not implemented.
		/// Maps to <see cref="Packets.ServerPacketType.AssetPayload"/>.
		/// </summary>
		void SendAssetPayload( object payload );

		/// <summary>
		/// Informs the client that its respective entity has
		/// been spawned in the world, and sends the latest
		/// game state snapshot.
		/// </summary>
		void SendSpawnResponse( int clientEntityId );

		/// <summary>
		/// Maps to <see cref="Packets.ServerPacketType.SpawnCompleteAck"/>.
		/// </summary>
		void SendSpawnCompleteAck();

		/// <summary>
		/// Not implemented.
		/// 
		/// Maps to <see cref="Packets.ServerPacketType.GameStatePayload"/>.
		/// </summary>
		void SendGameStatePayload();

		/// <summary>
		/// Not implemented.
		/// 
		/// Maps to <see cref="Packets.ServerPacketType.GameEventPayload"/>.
		/// </summary>
		void SendGameEventPayload( int eventId, object[]? args );
		#endregion
	}
}
