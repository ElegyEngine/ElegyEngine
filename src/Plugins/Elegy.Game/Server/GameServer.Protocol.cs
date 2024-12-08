// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Session;
using Game.Shared;
using System.Net;

namespace Game.Server
{
	public partial class GameServer
	{
		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.JoinRequest"/>.
		/// </summary>
		public void ReceiveJoinRequest( IPAddress address )
		{
			int clientId = GetClientId( address );
			if ( clientId < 0 )
			{
				mLogger.Error( $"Received input snapshot from an invalid client ({clientId}, '{address}')" );
				return;
			}

			var client = Connections[clientId];

			// If this client is also the local host, we don't need to do
			// any checks, just jump right into the game
			if ( client.Bridge.Bypass )
			{
				ReceiveSpawnRequest( clientId );
				return;
			}

			ConnectionModify( clientId, GameSessionState.Authenticating );
			client.Bridge.SendAuthRequest( false );
		}

		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.AuthResponse"/>.
		/// </summary>
		public void ReceiveAuthResponse( int clientId, string? password )
		{
			var client = Connections[clientId];

			// TODO: password protection

			client.Bridge.SendAssetInfoRequest( AssetCache.Registry );
		}

		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.AssetInfoResponse"/>.
		/// </summary>
		public void ReceiveAssetInfoResponse( int clientId, Span<int> missingAssetIds, Span<int> assetHashes )
		{
			if ( !missingAssetIds.IsEmpty )
			{
				ConnectionTerminate( clientId,
					$"You are missing {missingAssetIds.Length} assets.\nDownloading them is not supported yet." );
				return;
			}

			// TODO: check asset hashes

			ConnectionModify( clientId, GameSessionState.Downloading );
			Connections[clientId].Bridge.SendAssetPayload( 0 );
		}

		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.SpawnRequest"/>.
		/// </summary>
		public void ReceiveSpawnRequest( int clientId )
		{
			int entityId = SpawnClientEntity( clientId );

			ConnectionModify( clientId, GameSessionState.Joining );
			Connections[clientId].Bridge.SendSpawnResponse( entityId );
		}

		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.SpawnComplete"/>.
		/// </summary>
		public void ReceiveSpawnComplete( int clientId )
		{
			ConnectionModify( clientId, GameSessionState.Connected );
			// Let the client know that it may begin sending input snapshots now
			Connections[clientId].Bridge.SendSpawnCompleteAck();
		}

		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.InputPayload"/>.
		/// </summary>
		public void ReceiveInputPayload( int clientId, in ClientCommands snapshot )
		{
			ClientConnection client = Connections[clientId];

			if ( client.State != GameSessionState.Connected )
			{
				mLogger.Warning( $"Received input snapshot from a client that hasn't yet spawned" );

				// TODO: implement CVars
				// if ( Strict )
				//{
				//	ConnectionTerminate( clientId, "Illegal operation" );
				//}

				return;
			}

			Connections[clientId].AddInputSnapshot( snapshot );
		}

		/// <summary>
		/// See <see cref="Session.Packets.ClientPacketType.EventPayload"/>.
		/// </summary>
		public void ReceiveEventPayload( int clientId, int entityId, int eventId, object[]? args )
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// A bad packet was detected.
		/// </summary>
		public void ReceiveInvalidPacket( int clientId )
		{
			ConnectionTerminate( clientId, "Bad packet, sorry" );
		}
	}
}
