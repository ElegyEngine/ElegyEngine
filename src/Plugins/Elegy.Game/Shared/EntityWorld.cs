// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace Game.Shared
{
	public class EntityWorld
	{
		public bool AllSpawned { get; set; } = false;

		public fennecs.World EcsWorld { get; }

		public Entity[] Entities { get; }

		public IEnumerable<Entity> AliveEntities
		{
			get
			{
				for ( int i = 0; i < Entities.Length; i++ )
				{
					if ( Entities[i].Alive )
					{
						yield return Entities[i];
					}
				}
			}
		}

		public EntityWorld( int capacity = 4096 )
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

			foreach ( var index in Enumerable.Range( 0, Entities.Length ) )
			{
				Entities[index] = new( index );
			}
		}

		public void Shutdown()
		{
		}

		public EntityBuilder CreateEntity()
		{
			int newEntityId = 0;
			foreach ( var entity in Entities.AsSpan() )
			{
				if ( !entity.Alive )
				{
					newEntityId = entity.Id;
					break;
				}
			}

			Entities[newEntityId] = new( this, newEntityId );
			return new( ref Entities[newEntityId] );
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity GetEntity( int id )
		{
			return Entities[id];
		}

		public void Dispatch<T>( T data ) where T : notnull
			=> EntityUtilities.DispatchGroup<T>( EcsWorld, data );
	}
}
