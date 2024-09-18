// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Shared;
using System.Net;

namespace Game.Session
{
	/// <summary>
	/// Handles communication between the server's world and
	/// the client's snapshot of the same.
	/// </summary>
	public interface IServerBridge
	{
		/// <summary>
		/// In singleplayer: does nothing really.
		/// In multiplayer: receives gamestate packets from the server.
		/// </summary>
		void Update( float delta );

		#region Properties
		/// <summary>
		/// Returns an asset registry for asset string bookkeeping.
		/// In singleplayer, it returns the server's own copy directly.
		/// In multiplayer, it constructs itself from the server's packets.
		/// </summary>
		AssetRegistry AssetRegistry { get; }

		/// <summary>
		/// In singleplayer, returns either <see cref="GameSessionState.Disconnected"/> or
		/// <see cref="GameSessionState.Connected"/>.
		/// In multiplayer, it may be any of the <see cref="GameSessionState"/> values.
		/// </summary>
		GameSessionState ConnectionState { get; }
		#endregion

		#region Protocol
		/// <summary>
		/// In singleplayer, immediately hooks onto the server.
		/// In multiplayer, sends packets in an attempt to connect.
		/// 
		/// Maps to <see cref="Packets.ClientPacketType.JoinRequest"/>.
		/// </summary>
		void SendJoinRequest( IPAddress address );

		/// <summary>
		/// Maps to <see cref="Packets.ClientPacketType.AuthResponse"/>.
		/// </summary>
		void SendAuthResponse( string? password );

		/// <summary>
		/// Maps to <see cref="Packets.ClientPacketType.AssetInfoResponse"/>.
		/// </summary>
		void SendAssetInfoResponse( Span<int> missingAssetIds, Span<int> assetHashes );

		/// <summary>
		/// Maps to <see cref="Packets.ClientPacketType.SpawnRequest"/>.
		/// </summary>
		void SendSpawnRequest();

		/// <summary>
		/// Maps to <see cref="Packets.ClientPacketType.SpawnComplete"/>.
		/// </summary>
		void SendSpawnComplete();

		/// <summary>
		/// Sends a <see cref="ClientCommands"/> snapshot to the server.
		/// This is performed by clients multiple times per second.
		/// 
		/// In singleplayer, this directly copies <paramref name="snapshot"/>
		/// into the server's client snapshot buffer.
		/// In multiplayer, it emits it as a packet.
		/// </summary>
		void SendInputPayload( in ClientCommands snapshot );

		/// <summary>
		/// Not implemented.
		/// 
		/// Maps to <see cref="Packets.ClientPacketType.EventPayload"/>.
		/// </summary>
		void SendEventPayload( int entityId, int eventId, object[]? args );
		#endregion
	}
}
