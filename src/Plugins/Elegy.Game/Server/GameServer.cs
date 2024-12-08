// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Utilities;
using Elegy.ConsoleSystem;
using Game.Shared;
using Game.Shared.Components;
using System.Net;
using Game.Session;

namespace Game.Server
{
	public partial class GameServer
	{
		private TaggedLogger mLogger = new( "Server" );
		private GameSession? mGameSession;

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

		public GameServer( int maxPlayers, GameSession? gameSession = null )
		{
			Connections = new( maxPlayers );
			mGameSession = gameSession;
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
					client.Bridge.SendGameStatePayload();
				}
			} );
		}

		public bool Setup( ElegyMapDocument level )
		{
			// Step 1: Create entities
			Entity.OnMapLoadEvent mapLoadEvent = new( level );
			foreach ( var entityEntry in level.Entities )
			{
				EntityWorld.CreateEntity()
					.LoadKeyvalues( entityEntry.Attributes )
					.BuildArchetypes()
					.Dispatch( mapLoadEvent )
					.FinishSpawning(); // Dispatches Entity.SpawnEvent
			}

			// Step 2: Now that everybody has spawned, do another
			// run, e.g. accumulating spawnPointEntity links etc.
			EntityWorld.ForEachEntity( static entity =>
			{
				entity.Dispatch<Entity.PostSpawnEvent>( new( entity ) );
			} );

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
