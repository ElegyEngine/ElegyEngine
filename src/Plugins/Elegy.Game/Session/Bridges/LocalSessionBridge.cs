// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Server.Bridges;
using Game.Shared;
using System.Net;

namespace Game.Session.Bridges
{
	public class LocalSessionBridge : IServerBridge
	{
		private Server.GameServer mServer;
		private IPAddress Address => IPAddress.IPv6Loopback;
		private int mClientId = 0;

		private GameSessionState GetConnectionState()
		{
			int clientId = mServer.GetClientId( Address );
			return mServer.Connections[clientId].State;
		}

		public AssetRegistry AssetRegistry
			=> mServer.AssetRegistry;

		public GameSessionState ConnectionState
			=> GetConnectionState();

		public LocalSessionBridge( Server.GameServer server, GameSession session )
		{
			mServer = server;
			mServer.ConnectionStart( Address, new LocalClientBridge( session ) );
			mClientId = mServer.GetClientId( Address );
		}

		public void Update( float delta )
		{
			// Absolutely nothing here
		}

		#region Protocol
		public void SendJoinRequest( IPAddress address )
			=> mServer.ReceiveJoinRequest( Address );

		public void SendAuthResponse( string? password )
			=> mServer.ReceiveAuthResponse( mClientId, password );

		public void SendAssetInfoResponse( Span<int> missingAssetIds, Span<int> assetHashes )
			=> mServer.ReceiveAssetInfoResponse( mClientId, missingAssetIds, assetHashes );

		public void SendSpawnRequest()
			=> mServer.ReceiveSpawnRequest( mClientId );

		public void SendSpawnComplete()
			=> mServer.ReceiveSpawnComplete( mClientId );

		public void SendInputPayload( in ClientCommands snapshot )
			=> mServer.ReceiveInputPayload( mClientId, snapshot );

		public void SendEventPayload( int entityId, int eventId, object[]? args )
			=> mServer.ReceiveEventPayload( mClientId, entityId, eventId, args );
		#endregion
	}
}
