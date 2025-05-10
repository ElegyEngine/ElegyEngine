// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
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
			mStopwatch = Stopwatch.StartNew();
		}

		public void Shutdown()
		{
		}

		private DeltaTimer mSnapshotTimer;
		private DeltaTimer mUpdateTimer;
		private Stopwatch mStopwatch;
		private double CurrentSeconds => (double)mStopwatch.ElapsedTicks / Stopwatch.Frequency;

		public void Update( float delta )
		{
			if ( delta > 0.2f )
			{
				// Might as well discard the frame
				return;
			}
			
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
				mLogger.Log( "ServerUpdate" );
				
				float updateDelta = MathF.Max( delta, ServerUpdateTime );
				
				// These two will make transforms dirty
				double physicsStart = CurrentSeconds;
				PhysicsWorld.UpdateSimulation( updateDelta );
				
				double serverUpdateStart = CurrentSeconds;
				EntityWorld.Dispatch( new Entity.ServerUpdateEvent( this, updateDelta ) );
				
				// This one will listen to the changed transforms
				double transformListenStart = CurrentSeconds; 
				EntityWorld.Dispatch( new Entity.ServerTransformListenEvent( this, updateDelta ) );

				// Finally, this query clears them all
				double clearTransformsStart = CurrentSeconds;
				EntityWorld.EcsWorld.Stream<Transform>().For( static ( ref Transform t ) => { t.TransformDirty = false; } );
				double serverUpdateEnd = CurrentSeconds;
				
				double physicsMs = (serverUpdateStart - physicsStart) * 1000.0;
				double serverUpdateMs = (transformListenStart - serverUpdateStart) * 1000.0;
				double transformListenMs = (clearTransformsStart - transformListenStart) * 1000.0;
				double clearTransformsMs = (serverUpdateEnd - clearTransformsStart) * 1000.0;
				mLogger.Log( "Perf stats:" );
				mLogger.Log( $"Physics:         {physicsMs:F3} ms" );
				mLogger.Log( $"ServerUpdate:    {serverUpdateMs:F3} ms" );
				mLogger.Log( $"TransformListen: {transformListenMs:F3} ms" );
				mLogger.Log( $"ClearTransform:  {clearTransformsMs:F3} ms" );
			} );

			mSnapshotTimer.Seconds = GameSnapshotTime;
			mSnapshotTimer.Update( delta, () =>
			{
				mLogger.Log( "GameSnapshot" );
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
