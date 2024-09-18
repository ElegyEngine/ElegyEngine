// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;
using Game.Client;
using Game.Server;

namespace Game.Shared.Components
{
	[Component]
	[Requires<Transform>]
	public partial struct Player
	{
		public Player() { }

		public IPlayerControllable Controller { get; set; } = new BasicController();

		[EntityEvent<Entity.SpawnEvent>]
		public void Spawn()
		{
			
		}

		// This event attrib doesn't do anything at the moment,
		// ServerUpdate and ClientUpdate are called manually elsewhere
		[SystemEvent<Entity.ServerUpdateEvent>]
		public static void ServerUpdate( EntityWorld world, float delta )
		{
			// Update all player comps on the server
			//world.Query( ... );
		}

		[EntityEvent<Entity.ClientUpdateEvent>]
		public void ClientUpdate( GameClient client, float delta )
		{

		}
	}
}
