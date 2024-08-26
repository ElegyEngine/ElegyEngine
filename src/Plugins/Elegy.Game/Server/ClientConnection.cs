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
		/// <summary>
		/// ID of the client. Client 0 is typically a host in a LAN game.
		/// </summary>
		public int Id { get; set; } = 0;

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
		public List<ClientCommands> InputSnapshots { get; set; } = new( 64 );

		// TODO: have an e.g. ENet peer object here so we can actually interact with
		// clients over the network, kick them etc.
	}
}
