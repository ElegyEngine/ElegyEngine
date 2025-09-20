// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.LogSystem;
using Game.Shared;
using System.Net;
using Elegy.Common.Utilities;

namespace Game.Session.Bridges
{
	public class RemoteSessionBridge : IServerBridge
	{
		private TaggedLogger mLogger = new( "RemoteSession" );
		private AssetRegistry mRegistry = new();
		private GameSession mSession;
		private GameSessionState mState = GameSessionState.Disconnected;
		//private Connection mConnection;

		public AssetRegistry AssetRegistry => mRegistry;

		public GameSessionState ConnectionState => mState;

		public RemoteSessionBridge( GameSession session, IPAddress hostAddress )
		{
			mSession = session;
			//mConnection = new( hostAddress );
		}

		public void Update( float delta )
		{
			//var packets = mConnection.PollPackets();
			//foreach ( var packet in packets )
			//{
			//	ProcessPacket( packet.Data );
			//}
		}

		private void ProcessPacket( object data )
		{
			switch ( data )
			{
				// Server is asking us to type a password :)
				//case Server.Packets.AuthenticationPacket:
				//	mState = GameSessionState.Authenticating;
				//	break;

				// Server is sending asset data! Or a URL to them anyway
				//case Server.Packets.AssetPacket: break;

				// Uh oh we're being kicked
				case Server.Packets.DisconnectPacket disconnect:
					mSession.ReceiveDisconnect( disconnect.Reason );
					mState = GameSessionState.Disconnected;
					break;

				// Server is sending entity data yaayy :blobjump:
				//case Server.Packets.GameStatePacket: break;

				// Server is sending entity events let's go
				//case Server.Packets.GameEventPacket: break;

				default:
					mLogger.Error( "Received data is not a valid game packet" );
					break;
			}
		}

		#region Protocol
		public void SendJoinRequest( IPAddress address )
		{
			//mConnection.Send( new Packets.JoinRequest() );
			mState = GameSessionState.Joining;
		}

		public void SendAuthResponse( string? password )
		{
			//mConnection.Send( new Packets.AuthResponse( password ) );
		}

		public void SendAssetInfoResponse( Span<int> missingAssetIds, Span<int> assetHashes )
		{
			//mConnection.Send( new Packets.AssetInfoResponse( Address, missingAssetIds, assetHashes ) );
		}

		public void SendSpawnRequest()
		{
			//mServer.Send( new Packets.SpawnRequest() );
		}

		public void SendSpawnComplete()
		{
			//mServer.Send( new Packets.SpawnComplete() );
		}

		public void SendInputPayload( in ClientCommands snapshot )
		{
			//mConnection.Send( new Packets.InputPayloadPacket( snapshot ) );
		}

		public void SendEventPayload( int entityId, int eventId, object[]? args )
		{
			//mServer.Send( new Packets.EventPayloadPacket( entityId, eventId, args ) );
		}
		#endregion
	}
}
