// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ConsoleSystem;
using Elegy.ECS;
using Game.Client;
using Game.Server;
using Game.Shared.Components;
using System.Runtime.CompilerServices;
using EcsEntity = fennecs.Entity;

namespace Game.Shared
{
	public struct EntityHandle
	{
		public int EntityId { get; }
		public EntityWorld World { get; }

		public EntityHandle( Entity entity )
		{
			EntityId = entity.Id;
			World = entity.World;
		}

		public Entity Entity
			=> World.Entities[EntityId];

		public bool Alive
			=> World.Entities[EntityId].Alive;
	}

	public ref struct EntityBuilder
	{
		ref Entity mEntity;

		public EntityBuilder( ref Entity entity )
		{
			mEntity = ref entity;
		}

		public EntityBuilder LoadKeyvalues( Dictionary<string, string> properties )
		{
			mEntity.LoadFromKeyvalues( properties );
			return this;
		}

		public EntityBuilder With<T>() where T : notnull, new()
		{
			mEntity.RefOrCreate<T>();
			return this;
		}

		public ref Entity FinishSpawning()
		{
			EntityUtilities.FinishSpawningEntity( mEntity.EcsObject );
			return ref mEntity;
		}
	}

	public partial struct Entity
	{
		private static TaggedLogger mLogger = new( "Entity" );

		[EventModel] public record struct TouchEvent( Entity Self, Entity Other );
		[EventModel] public record struct TouchHoldEvent( Entity Self, Entity Other );
		[EventModel] public record struct TouchEndEvent( Entity Self, Entity Other );
		[EventModel] public record struct ClientPossessedEvent( Entity Self );
		[EventModel] public record struct SpawnEvent( Entity Self );
		[EventModel] public record struct PostSpawnEvent( Entity Self );
		[EventModel] public record struct PrecacheEvent( Entity Self );
		[EventModel] public record struct ClientUpdateEvent( Entity Self, GameClient Client, float Delta );
		[EventModel] public record struct ServerUpdateEvent( GameServer Server, float Delta );
		[EventModel] public record struct OnMapLoadEvent( Entity Self );
		
		public int Id { get; }
		public EntityWorld World { get; }
		public EcsEntity EcsObject { get; }
		public Archetype Archetype => EcsObject.Ref<Archetype>();
		public bool Alive => EcsObject.Alive;

		public Entity( int id )
		{
			Id = id;
		}

		public Entity( EntityWorld world, int id )
		{
			Id = id;
			World = world;

			EcsObject = world.EcsWorld.Spawn();
			EcsObject.Add( this );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Destroy()
		{
			EcsObject.Despawn();
		}

		public void LoadFromKeyvalues( Dictionary<string, string> keys )
		{
			foreach ( var pair in keys )
			{
				// We must handle a few special cases here
				// TODO: target*, angle, angles etc.
				switch ( pair.Key )
				{
					case "targetname":
						//EntityUtilities
						//	.CreateOrRef<Target>( EcsObject )
						//	.Name = pair.Value;
						break;

					case "origin":
						EntityUtilities
							.CreateOrRef<Transform>( EcsObject )
							.Position = Parse.Vector3( pair.Value );
						break;

					default:
						if ( !EntityUtilities.ParseComponentKeyvalue( EcsObject, pair.Key, pair.Value ) )
						{
							mLogger.Warning( $"Unknown keyvalue '{pair.Key}'!" );
						}
						break;
				}
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ref T RefOrCreate<T>() where T : notnull, new()
			=> ref EntityUtilities.CreateOrRef<T>( EcsObject );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Has<T>() where T : notnull
			=> EcsObject.Has<T>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ref T Ref<T>()
			=> ref EcsObject.Ref<T>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Dispatch<T>( T param ) where T : notnull
			=> EntityUtilities.DispatchEvent<T>( EcsObject, param );

		public void DispatchNamed( ReadOnlySpan<char> name )
			=> EntityUtilities.DispatchNamedEvent( EcsObject, name, static message =>
			{
				mLogger.Warning( message );
			} );
	}
}
