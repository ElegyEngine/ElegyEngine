// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

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

		public void ConnectionModify( int clientId, GameSessionState newState )
		{
			var client = Connections[clientId];

			mLogger.Verbose( $"Connection '{client.Address}' modified, new state: {newState}" );
			Connections[clientId].State = newState;
		}

		public void ConnectionTerminate( int clientId, string reason = "Unknown error" )
		{
			var client = Connections[clientId];

			client.Bridge.SendDisconnect( reason );
			ConnectionModify( clientId, GameSessionState.Disconnected );
		}

		public int GetClientId( IPAddress clientAddress )
		{
			foreach ( var connection in Connections )
			{
				if ( connection.Address == clientAddress
					&& connection.State != GameSessionState.Disconnected )
				{
					return connection.Id;
				}
			}

			return -1;
		}
	}
}
