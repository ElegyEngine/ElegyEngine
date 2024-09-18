// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Shared;
using System.Net;

namespace Game.Server.Bridges
{
	public class RemoteClientBridge : IClientBridge
	{
		private GameServer mServer;
		private IPAddress mAddress;
		private int mClientId;
		//private Connection mConnection;

		public bool Bypass => IPAddress.IsLoopback( mAddress );

		public RemoteClientBridge( GameServer server, IPAddress clientAddress )
		{
			mServer = server;
			mAddress = clientAddress;
		}

		public void Update( float delta )
		{
			//var packets = mConnection.PollData();
			//foreach ( var packet in packets )
			//{
			//	ProcessPacket( packet.Data );
			//}
		}

		private void ProcessPacket( object data )
		{
			switch ( data )
			{
				default:
					mServer.ReceiveInvalidPacket( mClientId );
					break;
			}
		}

		public void SendDisconnect( string reason )
		{
			//mConnection.Send( new Packets.DisconnectPacket( reason ) );
		}

		public void SendAuthRequest( bool passwordNeeded )
		{
			//mConnection.Send( new Packets.AuthRequestPacket( passwordNeeded ) );
		}

		public void SendAssetInfoRequest( AssetRegistry registry )
		{
			//mConnection.Send( new Packets.AssetInfoRequestPacket( registry ) );
		}

		public void SendAssetPayload( object payload )
		{
			//mConnection.Send( new Packets.AssetPayloadPacket( payload ) );
		}

		public void SendSpawnResponse( int clientEntityId, EntityWorld world )
		{
			//mConnection.Send( new Packets.SpawnResponsePacket( clientEntityId, world ) );
		}

		public void SendSpawnCompleteAck()
		{
			//mConnection.Send( new Packets.SpawnCompleteAck() );
		}

		public void SendGameStatePayload( EntityWorld world )
		{
			//mConnection.Send( new Packets.GameStatePayloadPacket( world ) );
		}

		public void SendGameEventPayload( int eventId, object[]? args )
		{
			throw new NotImplementedException();
		}
	}
}
