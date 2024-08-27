// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Session;

namespace Game.Shared.Bridges
{
	public class LocalSessionBridge : ISessionBridge
	{
		private Server.GameServer mServer;

		public LocalSessionBridge( Server.GameServer server )
		{
			mServer = server;
		}

		public AssetRegistry GetAssetRegistry()
		{
			throw new NotImplementedException();
		}

		public List<Entity> GetEntities()
			=> mServer.EntityWorld.Entities;

		public void SendInputSnapshot( in ClientCommands snapshot )
		{
			// We assume that the host (e.g. in a LAN game)
			// is always client 0. This also applies to singleplayer.
			mServer.AcceptClientInput( 0, snapshot );
		}

		public void Update( float delta )
		{
			// *shrug*
		}
	}
}
