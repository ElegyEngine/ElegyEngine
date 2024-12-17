// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Session;
using Game.Shared;

namespace Game.Server.Bridges
{
	public class LocalClientBridge : IClientBridge
	{
		private readonly GameSession mSession;

		public LocalClientBridge( GameSession session )
		{
			mSession = session;
		}

		public void Update( float delta ) { }

		public bool Bypass => true;

		#region Protocol
		public void SendDisconnect( string reason )
			=> mSession.ReceiveDisconnect( reason );

		public void SendAuthRequest( bool passwordNeeded )
		{
			// Bypassed
		}

		public void SendAssetInfoRequest( AssetRegistry registry )
		{
			// Bypassed
		}

		public void SendAssetPayload( object payload )
		{
			// Bypassed
		}

		public void SendSpawnResponse( int clientEntityId )
			=> mSession.ReceiveSpawnResponse( clientEntityId );

		public void SendSpawnCompleteAck()
			=> mSession.ReceiveSpawnCompleteAck();

		public void SendGameStatePayload()
		{
			// TODO: 
		}

		public void SendGameEventPayload( int eventId, object[]? args )
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
