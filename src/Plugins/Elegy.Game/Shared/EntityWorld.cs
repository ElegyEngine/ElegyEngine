// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace Game.Shared
{
	public static class EntityWorld
	{
		private static int mNumEntitySlots;

		public static bool AllSpawned { get; set; } = false;
		public static fennecs.World EcsWorld { get; private set; }
		public static Entity[] Entities { get; private set; }
		public static fennecs.Entity[] EcsObjects { get; private set; }

		public static event Action<Entity> OnSpawned = delegate { };
		public static event Action<Entity> OnPreSpawned = delegate { };
		public static event Action<Entity> OnDestroyed = delegate { };
		public static event Action<Entity> OnPreDestroyed = delegate { };

		public static void Init( int capacity = 4096 )
		{
			EcsWorld = new( capacity )
			{
				Name = "SharedEntityEcsWorld",
				GCBehaviour = fennecs.World.GCAction.ManualOnly
							  | fennecs.World.GCAction.CompactStagnantArchetypes
							  | fennecs.World.GCAction.DisposeEmptyArchetypes
							  | fennecs.World.GCAction.DisposeEmptyRelationArchetypes
			};

			Entities = new Entity[capacity];
			EcsObjects = new fennecs.Entity[capacity];
		}

		public static void Shutdown()
		{
		}

		public static EntityBuilder CreateEntity()
		{
			int newEntityId = -1;
			for ( int i = 0; i < mNumEntitySlots; i++ )
			{
				if ( !EcsObjects[i].Alive )
				{
					newEntityId = i;
					break;
				}
			}

			if ( newEntityId < 0 )
			{
				newEntityId = mNumEntitySlots;
			}
			mNumEntitySlots++;
			
			EcsObjects[newEntityId] = EcsWorld.Spawn();
			Entities[newEntityId] = new( newEntityId );
			OnPreSpawned( Entities[newEntityId] );
			return new( ref Entities[newEntityId] );
		}

		public static void FinishSpawning( int entityId )
			=> OnSpawned( Entities[entityId] );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity GetEntity( int id )
			=> Entities[id];

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static ref Entity GetEntityRef( int id )
			=> ref Entities[id];

		public static void DestroyEntity( int id )
		{
			OnPreDestroyed( Entities[id] );
			Entities[id].EcsObject.Despawn();
			OnDestroyed( Entities[id] );
		}

		public static fennecs.Entity GetEcsObject( int id )
			=> EcsObjects[id];

		public static ref fennecs.Entity GetEcsObjectRef( int id )
			=> ref EcsObjects[id];

		public static void ForEachEntity( Action<Entity> action )
		{
			for ( int i = 0; i < mNumEntitySlots; i++ )
			{
				if ( !Entities[i].Alive )
				{
					continue;
				}

				action( Entities[i] );
			}
		}

		public static void Dispatch<T>( T data ) where T : notnull
			=> EntityUtilities.DispatchGroup( EcsWorld, data );
	}
}
