// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;
using Game.Client;
using Game.Server;
using Game.Shared.Components;
using System.Runtime.CompilerServices;
using Elegy.Common.Assets;

namespace Game.Shared
{
	public readonly struct EntityHandle
	{
		public int EntityId { get; }

		public EntityHandle( Entity entity )
		{
			EntityId = entity.Ref<EntitySlot>().Id;
		}

		public ref Entity Entity
			=> ref EntityWorld.Entities[EntityId];

		public bool Alive => Entity.Alive;
	}

	public readonly ref struct EntityBuilder
	{
		private static TaggedLogger mLogger = new( "EntityBuilder" );

		// While spawning, an ECS entity will change its generation, archetype etc.
		// and it's all part of a 64-bit signature. Storing an ID & pulling from that
		// means we're getting the latest iteration of that signature, i.e. entity
		private ref Entity Entity => ref EntityWorld.GetEntity( mId );
		private readonly int mId;

		public EntityBuilder( ref Entity entity )
		{
			mId = entity.Ref<EntitySlot>().Id;
		}

		public EntityBuilder PrepareForKeyvalues( Dictionary<string, string> properties )
		{
			ref var entity = ref Entity;

			foreach ( var pair in properties )
			{
				switch ( pair.Key )
				{
					case "classname":
						if ( pair.Value == "worldspawn" )
						{
							entity.RefOrCreate<Worldspawn>();
						}
						break;

					case "targetname":
						entity.RefOrCreate<Name>();
						break;

					case "origin":
						entity.RefOrCreate<Transform>();
						break;

					case "model":
						entity.RefOrCreate<StaticModel>();
						break;

					case "cmodel":
					case "angle":
					case "angles":
					case "mapversion":
					case "_tb_textures":
					case "_generator":
						break;

					default:
						if ( !EntityUtilities.PrepareComponentForKeyvalue( ref entity, pair.Key ) )
						{
							mLogger.Warning( $"Unknown keyvalue '{pair.Key}'!" );
						}

						break;
				}
			}

			return this;
		}

		public EntityBuilder LoadKeyvalues( Dictionary<string, string> properties )
		{
			ref var entity = ref Entity;

			foreach ( var pair in properties )
			{
				// We must handle a few special cases here, like model and cmodel
				// TODO: angles etc.
				switch ( pair.Key )
				{
					case "targetname":
						entity.Ref<Name>().Targetname = pair.Value;
						break;

					case "origin":
						entity.Ref<Transform>().Position = Parse.Vector3( pair.Value );
						break;

					case "model":
						entity.Ref<StaticModel>().Model = ModelProperty.BrushVisual( int.Parse( pair.Value[1..] ) );
						break;

					case "cmodel":
						int meshId = int.Parse( pair.Value[1..] );
						if ( entity.Has<Body>() )
						{
							entity.Ref<Body>().CollisionModel = ModelProperty.BrushCollision( meshId );
						}
						else if ( entity.Has<BodyStatic>() )
						{
							entity.Ref<BodyStatic>().CollisionModel = ModelProperty.BrushCollision( meshId );
						}
						else if ( entity.Has<BodyKinematic>() )
						{
							entity.Ref<BodyKinematic>().CollisionModel = ModelProperty.BrushCollision( meshId );
						}
						break;

					default:
						EntityUtilities.ParseComponentKeyvalue( ref entity, pair.Key, pair.Value );
						break;
				}
			}

			return this;
		}

		public EntityBuilder With<T>() where T : notnull, new()
		{
			Entity.RefOrCreate<T>();
			return this;
		}

		public EntityBuilder BuildArchetypes()
		{
			EntityUtilities.FinishSpawningEntity( ref EntityWorld.GetEntity( mId ) );
			return this;
		}

		public EntityBuilder Dispatch<T>( T data ) where T : notnull
		{
			Entity.Dispatch( data );
			return this;
		}

		public ref Entity FinishSpawning()
		{
			EntityWorld.FinishSpawning( mId );
			return ref Entity;
		}
	}

	public static class EntityExtensions
	{
		private static TaggedLogger mLogger = new( "Entity" );

		public static Archetype GetArchetype( this Entity self )
			=> self.Ref<Archetype>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Dispatch<T>( this Entity self, T param ) where T : notnull
			=> EntityUtilities.DispatchEvent( self, param );
 
		public static bool DispatchNamed( this Entity self, ReadOnlySpan<char> name )
		{
			if ( !EntityUtilities.DispatchNamedEvent( self, name ) )
			{
				mLogger.Warning( $"Invalid input: '{name}'" );
				return false;
			}

			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static ref T RefOrCreate<T>( this ref Entity self ) where T : notnull, new()
			=> ref EntityUtilities.CreateOrRef<T>( ref self );
	}

	#region Events

	[EventModel]
	public record struct TouchEvent( Entity Self, Entity Other );

	[EventModel]
	public record struct TouchHoldEvent( Entity Self, Entity Other );

	[EventModel]
	public record struct TouchEndEvent( Entity Self, Entity Other );

	[EventModel]
	public record struct ClientPossessedEvent( Entity Self );

	[EventModel]
	public record struct DebugDrawEvent;

	[EventModel]
	public record struct SpawnEvent( Entity Self );

	[EventModel]
	public record struct ClientSpawnEvent( Entity Self );

	[EventModel]
	public record struct PostSpawnEvent( Entity Self );

	[EventModel]
	public record struct DespawnEvent( Entity Self );

	[EventModel]
	public record struct ClientDespawnEvent( Entity Self );

	[EventModel]
	public record struct ClientUpdateEvent( Entity Self, GameClient Client, float Delta );

	[EventModel]
	public record struct ServerUpdateEvent( GameServer Server, float Delta );

	[EventModel]
	public record struct ServerTransformListenEvent( GameServer Server, float Delta );

	[EventModel]
	public record struct OnMapLoadEvent( ElegyMapDocument MapDocument );

	#endregion

	public readonly struct EntitySlot
	{
		public required int Id { get; init; }
	}
}
