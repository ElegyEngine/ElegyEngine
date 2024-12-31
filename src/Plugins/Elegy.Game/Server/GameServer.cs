// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Utilities;
using Elegy.ConsoleSystem;
using Game.Shared;
using Game.Shared.Components;
using Game.Session;
using Game.Shared.Physics;

namespace Game.Server
{
	public partial class GameServer
	{
		private TaggedLogger mLogger = new( "Server" );

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

		public GameServer( int maxPlayers )
		{
			Connections = new( maxPlayers );
		}

		public void Shutdown()
		{
		}

		private DeltaTimer mSnapshotTimer;
		private DeltaTimer mUpdateTimer;

		public void Update( float delta )
		{
			foreach ( var connection in Connections )
			{
				if ( connection.State == GameSessionState.Disconnected )
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
				// These two will make transforms dirty
				PhysicsWorld.UpdateSimulation( ServerUpdateTime );
				EntityWorld.Dispatch( new Entity.ServerUpdateEvent( this, ServerUpdateTime ) );

				// This one will listen to the changed transforms
				EntityWorld.Dispatch( new Entity.ServerTransformListenEvent( this, ServerUpdateTime ) );

				// Finally, this query clears them all
				EntityWorld.EcsWorld.Stream<Transform>().For( static ( ref Transform t ) => { t.TransformDirty = false; } );
			} );

			mSnapshotTimer.Seconds = GameSnapshotTime;
			mSnapshotTimer.Update( delta, () =>
			{
				foreach ( var client in Connections )
				{
					client.Bridge.SendGameStatePayload();
				}
			} );
		}

		public bool Setup( ElegyMapDocument level )
		{
			AssetCache.InitLevel( level );

			// Step 1: Create entities
			Entity.OnMapLoadEvent mapLoadEvent = new( level );
			foreach ( var entityEntry in level.Entities )
			{
				EntityWorld.CreateEntity()
					.PrepareForKeyvalues( entityEntry.Attributes )
					.BuildArchetypes()
					.LoadKeyvalues( entityEntry.Attributes )
					.Dispatch( mapLoadEvent )
					.FinishSpawning(); // Dispatches Entity.SpawnEvent
			}

			// Step 2: Now that everybody has spawned, do another
			// run, e.g. accumulating spawnPointEntity links etc.
			EntityWorld.ForEachEntity( static entity => { entity.Dispatch<Entity.PostSpawnEvent>( new( entity ) ); } );

			// Just to kick things off a little bit
			PhysicsWorld.UpdateSimulation( 0.01f );

			return true;
		}

		public int SpawnClientEntity( int clientId )
		{
			mLogger.Log( $"Spawning client {clientId}, address '{Connections[clientId].Address}'" );

			Entity playerEntity = EntityWorld
				.CreateEntity()
				.With<Player>()
				.BuildArchetypes()
				.FinishSpawning();

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
