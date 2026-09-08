// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Elegy.Common.Utilities;
using Game.Shared.Components;

namespace Game.Shared
{
	public static class EntityWorld
	{
		private static TaggedLogger mLogger = new( "EntityWorld" );

		private static Stopwatch mStopwatch = new();
		private static int mNumEntitySlots;
		private static List<EntityOutputCommand> mOutputCommands;

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
			mStopwatch = Stopwatch.StartNew();

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
			mOutputCommands = new( 32 );
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

		// TODO: Upgrade to .NET 10 etc. and enjoy refs in lambdas
		public static void ForEachNamedEntity( string name, Action<Entity> what )
		{
			// Right now we're just doing a super naive solution. Each named entity has a Name component,
			// which has a string inside. If this becomes a bottleneck, we can speed it up with a dictionary
			// and maybe a dedicated targetname string allocator. Test this with hundreds of triggers etc.
			static void EntityLoop( (string targetName, Action<Entity> action) u,
				ref Entity self, ref Name nameComp )
			{
				if ( self.Alive && nameComp.Targetname.Equals( u.targetName ) )
				{
					u.action( self );
				}
			}

			EcsWorld.Stream<Entity, Name>().For( (name, what), EntityLoop );
		}

		public static void ForEachNamedEntity<TUniform>( string name, TUniform uniform, Action<TUniform, Entity> what )
		{
			// If you're confused by this, the "u" is basically a uniform value we pass to the ECS query. This allows
			// the callback to stay a static method, which is optimised. Having it non-static would allocate every call xwx
			// This version of ForEachNamedEntity follows that same principle. It allows you to pass any argument
			// that will remain uniform throughout the query, so you don't have to sacrifice speed or anything
			static void EntityLoop( (string targetName, TUniform uniform, Action<TUniform, Entity> action) u,
				ref Entity self, ref Name nameComp )
			{
				if ( self.Alive && nameComp.Targetname.Equals( u.targetName ) )
				{
					u.action( u.uniform, self );
				}
			}

			EcsWorld.Stream<Entity, Name>().For( (name, uniform, what), EntityLoop );
		}

		public static void ProcessAllOutputs()
		{
			long currentTime = mStopwatch.GetMicroseconds();

			Span<EntityOutputCommand> commands = mOutputCommands.AsSpan();
			for ( int i = 0; i < commands.Length; i++ )
			{
				// Oh boy, this one takes a while to type
				EntityUtilities.ComponentInput inputId = (EntityUtilities.ComponentInput)commands[i].InputId;
				long executionTime = commands[i].ExecutionTime;

				// "expired"
				if ( executionTime is long.MaxValue )
				{
					continue;
				}

				if ( currentTime < commands[i].ExecutionTime )
				{
					continue;
				}

				commands[i].ExecutionTime = long.MaxValue;
				EntityUtilities.DispatchNamedEvent( commands[i].Receiver, inputId );
			}
		}

		private static void FindFreeCommandOrAdd( in EntityOutputCommand command )
		{
			for ( int i = 0; i < mOutputCommands.Count; i++ )
			{
				if ( mOutputCommands[i].ExecutionTime is long.MaxValue )
				{
					mOutputCommands[i] = command;
					return;
				}
			}

			mOutputCommands.Add( command );
		}

		public static void QueueOutput( Entity sender, in EntityOutputEntry entry )
		{
			(fennecs.Entity Sender, long FireDelay, EntityUtilities.ComponentInput InputId) uniform;
			uniform.Sender = sender.EcsObject;
			uniform.FireDelay = (long)(entry.FireDelay * 1000.0f * 1000.0f);
			uniform.InputId = EntityUtilities.StringToInputId( entry.TargetInput );

			if ( uniform.InputId is EntityUtilities.ComponentInput.Invalid )
			{
				mLogger.Error( $"An entity tried firing {entry.TargetEntity}'s input '{entry.TargetInput}' - it does not exist" );
				return;
			}

			// TODO: This won't report non-existing entities...
			ForEachNamedEntity( entry.TargetEntity, uniform, static ( u, e ) =>
			{
				FindFreeCommandOrAdd( new()
				{
					Receiver = e.EcsObject,
					Sender = u.Sender,
					ExecutionTime = mStopwatch.GetMicroseconds() + u.FireDelay,
					InputId = (long)u.InputId
				} );
			} );
		}
	}
}
