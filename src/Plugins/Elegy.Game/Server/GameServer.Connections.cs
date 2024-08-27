// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Game.Shared;
using System.Net;

namespace Game.Server
{
	public partial class GameServer
	{
		public List<ClientConnection> Connections { get; } = new();
		
		public void ClientStartedConnecting( IPAddress clientAddress )
		{
			foreach ( var connection in Connections )
			{
				if ( connection.State == Session.GameSessionState.Disconnected )
				{
					connection.Address = clientAddress;
					connection.State = Session.GameSessionState.Connecting;
					return;
				}
			}

			Connections.Add( new()
			{
				Address = clientAddress,
				Id = Connections.Count,
				State = Session.GameSessionState.Connecting
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
