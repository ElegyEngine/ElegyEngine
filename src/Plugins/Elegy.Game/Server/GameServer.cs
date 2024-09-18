// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Game.Server
{
	public partial class GameServer
	{
		public int MaxPlayers => Connections.Capacity;

		/// <summary>
		/// How often (per second) to send game state packets to clients.
		/// </summary>
		public int GameSnapshotRate => 10;

		public bool Init()
		{
			if ( !EntityWorld.Init() )
			{
				return false;
			}

			return true;
		}

		public void Shutdown()
		{
			EntityWorld.Shutdown();
		}

		public void Update( float delta )
		{
			// TODO: listen to client connection packets

			EntityWorld.Update( delta );
		}

		public bool Setup( int maxPlayers, ElegyMapDocument level )
		{
			Connections.Clear();

			// TODO: later we could also obtain the level name etc.
			return EntityWorld.Setup( level );
		}
	}
}
