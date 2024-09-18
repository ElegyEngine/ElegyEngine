// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Utilities;
using Elegy.ConsoleSystem;

using Game.Shared;
using Game.Shared.Components;

using System.Net;

namespace Game.Server
{
	public partial class GameServer
	{
		private TaggedLogger mLogger = new( "Server" );

		public AssetRegistry AssetRegistry { get; } = new();
		public EntityWorld EntityWorld { get; }
		public int MaxPlayers => Connections.Capacity;

		/// <summary>
		/// How often (per second) to send game state packets to clients.
		/// </summary>
		public int GameSnapshotRate { get; set; } = 10;
		public float GameSnapshotTime => 1.0f / GameSnapshotRate;

		/// <summary>
		/// How often to update serverside entities.
		/// </summary>
		public int ServerUpdateRate { get; set; } = 40;
		public float ServerUpdateTime => 1.0f / ServerUpdateRate;

		public GameServer( EntityWorld world, int maxPlayers )
		{
			EntityWorld = world;
			Connections = new( maxPlayers );
		}

		public void Shutdown()
		{
		}

		private DeltaTimer mSnapshotTimer = new();
		private DeltaTimer mUpdateTimer = new();

		public void Update( float delta )
		{
			foreach ( var connection in Connections )
			{
				if ( connection.State == Session.GameSessionState.Disconnected )
				{
					continue;
				}

				connection.Bridge.Update( delta );
			}

			// We gotta constantly update these in case they're
			// changed via a CVar. It's an insignificant perf cost
			mUpdateTimer.Seconds = ServerUpdateTime;
			mUpdateTimer.Update( delta, () =>
			{
				EntityWorld.Dispatch(
					new Entity.ServerUpdateEvent( this, ServerUpdateTime ) );
			} );

			mSnapshotTimer.Seconds = GameSnapshotTime;
			mSnapshotTimer.Update( delta, () =>
			{
				foreach ( var client in Connections )
				{
					// Skip the local client
					if ( IPAddress.IsLoopback( client.Address ) )
					{
						continue;
					}

					//EmitGameState( client );
				}
			} );
		}

		public bool Setup( ElegyMapDocument level )
		{
			// Step 1: Create entities
			foreach ( var entityEntry in level.Entities )
			{
				EntityWorld.CreateEntity()
					.LoadKeyvalues( entityEntry.Attributes )
					.FinishSpawning();
			}

			// Step 2: Let entities load all their stuff
			foreach ( var entity in EntityWorld.AliveEntities )
			{
				entity.Dispatch<Entity.SpawnEvent>( new( entity ) );
			}

			// Step 3: Now that everybody has spawned, do another
			// run, e.g. accumulating spawnPointEntity links etc.
			foreach ( var entity in EntityWorld.AliveEntities )
			{
				entity.Dispatch<Entity.PostSpawnEvent>( new( entity ) );
			}

			return true;
		}

		public int SpawnClientEntity( int clientId )
		{
			mLogger.Log( $"Spawning client {clientId}, address '{Connections[clientId].Address}'" );

			Entity playerEntity = EntityWorld
				.CreateEntity()
				.With<Player>()
				.FinishSpawning();

			// TODO: defer this to later when the client fully joins?
			playerEntity.Dispatch<Entity.SpawnEvent>( new( playerEntity ) );

			SelectSpawnPoint( playerEntity );

			return playerEntity.Id;
		}

		private void SelectSpawnPoint( Entity player )
		{
			var spawnPointEntity = EntityWorld.Entities[SpawnPoint.SpawnPointIds.Random()];

			ref var playerTransform = ref player.Ref<Transform>();
			ref var spawnPoint = ref spawnPointEntity.Ref<SpawnPoint>();
			ref var spawnPointTransform = ref spawnPointEntity.Ref<Transform>();

			playerTransform.Position = spawnPointTransform.Position;
			spawnPoint.PlayerSpawned( new( player.Ref<Player>() ) );
		}
	}
}
