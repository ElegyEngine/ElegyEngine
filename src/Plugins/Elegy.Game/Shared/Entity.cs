// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Client;
using Game.Server;
using EcsEntity = fennecs.Entity;

namespace Game.Shared
{
	public struct Entity
	{
		public delegate void SpawnEvent();
		public delegate void PostSpawnEvent();
		public delegate void PrecacheEvent();
		public delegate void ClientUpdateEvent( GameClient client, float delta );
		public delegate void ServerUpdateEvent( EntityWorld world, float delta );

		public int Id { get; }
		public EntityWorld World { get; }
		public EcsEntity EcsObject { get; }

		public Entity( EntityWorld world, int id )
		{
			Id = id;
			World = world;

			EcsObject = world.EcsWorld.Spawn();
			EcsObject.Add( this );
		}

		public void LoadFromKeyvalues( Dictionary<string, string> keys )
		{
			
		}

		public void DispatchEvents<T>()
			where T: Delegate
		{

		}
	}
}
