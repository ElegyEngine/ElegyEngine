// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Session;
using Game.Shared;
using System.Net;

namespace Game.Server
{
	/// <summary>
	/// A serverside record of a client.
	/// </summary>
	public class ClientConnection
	{
		private int mConnectionRingIndex = 0;

		/// <summary>
		/// ID of the client. Client 0 is typically a host in a LAN game.
		/// </summary>
		public int Id { get; set; } = 0;

		/// <summary>
		/// Communication bridge with the client, either a direct in-memory
		/// link or one established by network packets.
		/// </summary>
		public IClientBridge Bridge { get; set; }

		/// <summary>
		/// The IP address of the client.
		/// </summary>
		public IPAddress Address { get; set; } = IPAddress.None;

		/// <summary>
		/// Connection state of the client.
		/// </summary>
		public GameSessionState State { get; set; } = GameSessionState.Disconnected;

		/// <summary>
		/// The server keeps track of the last 64 input snapshots from the client,
		/// partly for the purposes of rollback, partly for cheating.
		/// </summary>
		public ClientCommands[] InputSnapshots { get; } = new ClientCommands[64];

		public void AddInputSnapshot( ClientCommands snapshot )
		{
			InputSnapshots.RingAdd( snapshot, 64, mConnectionRingIndex++ );
		}

		public ClientCommands GetInputSnapshotInPast( int pastSteps = 1 )
		{
			return InputSnapshots.RingAt( 64, mConnectionRingIndex - pastSteps );
		}

		public ClientCommands GetLatestInputSnapshot()
		{
			return GetInputSnapshotInPast( 0 );
		}

		public void Renew( IPAddress address, IClientBridge bridge )
		{
			Address = address;
			Bridge = bridge;
			State = GameSessionState.Connecting;
		}
	}
}
