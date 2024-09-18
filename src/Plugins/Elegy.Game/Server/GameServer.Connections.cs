// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Game.Server.Bridges;
using Game.Session;
using Game.Shared;
using System.Net;

namespace Game.Server
{
	public partial class GameServer
	{
		public List<ClientConnection> Connections { get; }
		
		public void ConnectionStart( IPAddress clientAddress, IClientBridge? clientBridge = null )
		{
			if ( clientBridge is null )
			{
				clientBridge = new RemoteClientBridge( this, clientAddress );
			}

			mLogger.Log( $"New connection! ({clientAddress}, {clientBridge.GetType()})" );

			foreach ( var connection in Connections )
			{
				if ( connection.State == GameSessionState.Disconnected )
				{
					connection.Renew( clientAddress, clientBridge );
					return;
				}
			}

			Connections.Add( new()
			{
				Address = clientAddress,
				Id = Connections.Count,
				State = GameSessionState.Connecting,
				Bridge = clientBridge
			} );
		}

		public void AcceptClientInput( int clientId, in ClientCommands snapshot )
		{
			if ( clientId >= Connections.Count )
			{
				//mLogger.Error( $"Received input snapshot from an out-of-range client" );
				return;
			}

			ClientConnection client = Connections[clientId];

			if ( client.State != Session.GameSessionState.Connected )
			{
				//mLogger.Warning( $"Received input snapshot from a client that hasn't yet spawned" );
			}

			Connections[clientId].InputSnapshots.Add( snapshot );
		}
	}
}
