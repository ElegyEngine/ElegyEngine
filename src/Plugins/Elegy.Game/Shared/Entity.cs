// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ConsoleSystem;
using Elegy.ECS;
using Game.Client;
using Game.Server;
using Game.Shared.Components;
using System.Runtime.CompilerServices;
using Elegy.Common.Assets;
using Game.Session;
using EcsEntity = fennecs.Entity;

namespace Game.Shared
{
	public struct EntityHandle
	{
		public int EntityId { get; }

		public EntityHandle( Entity entity )
		{
			EntityId = entity.Id;
		}

		public Entity Entity
			=> EntityWorld.Entities[EntityId];

		public bool Alive
			=> EntityWorld.Entities[EntityId].Alive;
	}

	public ref struct EntityBuilder
	{
		private int mId;

		public EntityBuilder( ref Entity entity )
		{
			mId = entity.Id;
		}

		public EntityBuilder PrepareForKeyvalues( Dictionary<string, string> properties )
		{
			EntityWorld.GetEntityRef( mId ).CreateComponentsFromKeyvalues( properties );
			return this;
		}

		public EntityBuilder LoadKeyvalues( Dictionary<string, string> properties )
		{
			EntityWorld.GetEntityRef( mId ).LoadFromKeyvalues( properties );
			return this;
		}

		public EntityBuilder With<T>() where T : notnull, new()
		{
			EntityWorld.GetEntityRef( mId ).RefOrCreate<T>();
			return this;
		}

		public EntityBuilder BuildArchetypes()
		{
			EntityUtilities.FinishSpawningEntity( ref EntityWorld.GetEntityRef( mId ) );
			return this;
		}

		public EntityBuilder Dispatch<T>( T data ) where T : notnull
		{
			EntityWorld.GetEntityRef( mId ).Dispatch( data );
			return this;
		}

		public ref Entity FinishSpawning()
		{
			EntityWorld.FinishSpawning( mId );
			// Can't really return mEntity directly here, because there's now
			// two different copies of it, so we return the newer one
			return ref EntityWorld.GetEntityRef( mId );
		}
	}

	public struct Entity
	{
		private static TaggedLogger mLogger = new( "Entity" );

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
		public record struct OnMapLoadEvent( ElegyMapDocument MapDocument );

		#endregion

		public int Id { get; }
		public EcsEntity EcsObject => EntityWorld.GetEcsObject( Id );
		public ref EcsEntity EcsObjectRef => ref EntityWorld.GetEcsObjectRef( Id );
		public Archetype Archetype => EcsObject.Ref<Archetype>();
		public bool Alive => EcsObject.Alive;

		public Entity( int id )
		{
			Id = id;
			EcsObjectRef.Add( this );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void PreDestroyServer()
			=> Dispatch( new DespawnEvent( this ) );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void PreDestroyClient()
			=> Dispatch( new ClientDespawnEvent( this ) );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Destroy()
			=> EcsObjectRef.Despawn();

		public void CreateComponentsFromKeyvalues( Dictionary<string, string> keys )
		{
			foreach ( var pair in keys )
			{
				switch ( pair.Key )
				{
					case "targetname":
						//RefOrCreate<Target>();
						break;

					case "origin":
						RefOrCreate<Transform>();
						break;

					case "model":
						RefOrCreate<StaticModel>();
						break;

					default:
						if ( !EntityUtilities.PrepareComponentForKeyvalue( ref EcsObjectRef, pair.Key ) )
						{
							mLogger.Warning( $"Unknown keyvalue '{pair.Key}'!" );
						}

						break;
				}
			}
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
						//	.CreateOrRef<Target>( ref EcsObjectRef )
						//	.Name = pair.Value;
						break;

					case "origin":
						Ref<Transform>().Position = Parse.Vector3( pair.Value );
						break;

					case "model":
						Ref<StaticModel>().Model = new( pair.Value );
						break;

						break;

					default:
						EntityUtilities.ParseComponentKeyvalue( ref EcsObjectRef, pair.Key, pair.Value );
						break;
				}
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ref T RefOrCreate<T>() where T : notnull, new()
			=> ref EntityUtilities.CreateOrRef<T>( ref EcsObjectRef );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Has<T>() where T : notnull
			=> EcsObjectRef.Has<T>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ref T Ref<T>()
			=> ref EcsObjectRef.Ref<T>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Dispatch<T>( T param ) where T : notnull
			=> EntityUtilities.DispatchEvent<T>( EcsObject, param );

		public void DispatchNamed( ReadOnlySpan<char> name )
			=> EntityUtilities.DispatchNamedEvent( EcsObject, name, static message => { mLogger.Warning( message ); } );
	}
}
