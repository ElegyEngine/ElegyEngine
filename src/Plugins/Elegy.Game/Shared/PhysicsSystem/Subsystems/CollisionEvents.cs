using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using Elegy.Common.Utilities;
using Game.Shared.PhysicsSystem.Extensions;
using Game.Shared.PhysicsSystem.Interfaces;
using Parallel = Elegy.Common.Parallel.Parallel;

// In essence, the system works like this:
// 1) Collect collision events in IContactModifier.ConfigureContactManifold (Start and Touch events)
// 2) After collision detection, determine which bodies have stopped touching
// 3) Leave the results exposed to the end user, until the next simulation tick
// 4) Before the next collision detection, allocate caches for worker threads

namespace Game.Shared.PhysicsSystem.Subsystems
{
	/// <summary>
	/// 48 bytes for everything. Very neatly packed
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct CollisionData
	{
		public CollisionData( Contact c0, Contact c1, Contact c2, Contact c3 )
		{
			NormalAndFeatureId0 = new( c0.Normal, c0.FeatureId );
			NormalAndFeatureId1 = new( c1.Normal, c1.FeatureId );
			NormalAndFeatureId2 = new( c2.Normal, c2.FeatureId );
			NormalAndFeatureId3 = new( c3.Normal, c3.FeatureId );

			Offset0XYZAndOffset1X = new( c0.Offset.X, c0.Offset.Y, c0.Offset.Z, c1.Offset.X );
			Offset1YZAndOffset2XY = new( c1.Offset.Y, c1.Offset.Z, c2.Offset.X, c2.Offset.Y );
			Offset2ZAndOffset3XYZ = new( c2.Offset.Z, c3.Offset.X, c3.Offset.Y, c3.Offset.Z );
		}

		public Vector4 NormalAndFeatureId0;
		public Vector4 NormalAndFeatureId1;
		public Vector4 NormalAndFeatureId2;
		public Vector4 NormalAndFeatureId3;
		public Vector4 Offset0XYZAndOffset1X;
		public Vector4 Offset1YZAndOffset2XY;
		public Vector4 Offset2ZAndOffset3XYZ;

		public Vector3 Normal0 => NormalAndFeatureId0.ToVector3();
		public Vector3 Normal1 => NormalAndFeatureId1.ToVector3();
		public Vector3 Normal2 => NormalAndFeatureId2.ToVector3();
		public Vector3 Normal3 => NormalAndFeatureId3.ToVector3();

		public int FeatureId0 => (int)NormalAndFeatureId0.W;
		public int FeatureId1 => (int)NormalAndFeatureId1.W;
		public int FeatureId2 => (int)NormalAndFeatureId2.W;
		public int FeatureId3 => (int)NormalAndFeatureId3.W;

		public Vector3 Offset0 => new()
		{
			X = Offset0XYZAndOffset1X.X,
			Y = Offset0XYZAndOffset1X.Y,
			Z = Offset0XYZAndOffset1X.Z
		};

		public Vector3 Offset1 => new()
		{
			X = Offset0XYZAndOffset1X.W,
			Y = Offset1YZAndOffset2XY.X,
			Z = Offset1YZAndOffset2XY.Y
		};

		public Vector3 Offset2 => new()
		{
			X = Offset1YZAndOffset2XY.Z,
			Y = Offset1YZAndOffset2XY.W,
			Z = Offset2ZAndOffset3XYZ.X
		};

		public Vector3 Offset3 => new()
		{
			X = Offset2ZAndOffset3XYZ.Y,
			Y = Offset2ZAndOffset3XYZ.Z,
			Z = Offset2ZAndOffset3XYZ.W
		};
	}

	/// <summary>
	/// A per-body cache which helps us figure out if the body has received contacts from the previous body again.
	/// Supports up to 8 different colliders to keep track of.
	/// </summary>
	internal struct ContactTracker
	{
		public const int NoBody = -1;

		[MethodImpl( MethodImplOptions.AggressiveOptimization )]
		private bool TrySet( ref int target, int value, int id )
		{
			if ( target == NoBody || target == value )
			{
				target = value;
				FreshnessFlags |= (byte)(1 << id);
				return true;
			}

			return false;
		}

		[MethodImpl( MethodImplOptions.AggressiveOptimization )]
		private void ClearIfStale( ref int target, int id )
		{
			if ( (FreshnessFlags & (byte)(1 << id)) == 0 )
			{
				target = NoBody;
			}
		}

		public ContactTracker()
		{
		}

		/// <summary>
		/// Adds another body to track, and marks the body as fresh if it was already touched earlier.
		/// </summary>
		public void Add( int otherBody )
		{
			if ( TrySet( ref OtherBody0, otherBody, 0 ) ) return;
			if ( TrySet( ref OtherBody1, otherBody, 1 ) ) return;
			if ( TrySet( ref OtherBody2, otherBody, 2 ) ) return;
			if ( TrySet( ref OtherBody3, otherBody, 3 ) ) return;
			if ( TrySet( ref OtherBody4, otherBody, 4 ) ) return;
			if ( TrySet( ref OtherBody5, otherBody, 5 ) ) return;
			if ( TrySet( ref OtherBody6, otherBody, 6 ) ) return;
			if ( TrySet( ref OtherBody7, otherBody, 7 ) ) return;
		}

		/// <summary>
		/// Clears all bodies that aren't fresh.
		/// </summary>
		public void ClearStale()
		{
			ClearIfStale( ref OtherBody0, 0 );
			ClearIfStale( ref OtherBody1, 1 );
			ClearIfStale( ref OtherBody2, 2 );
			ClearIfStale( ref OtherBody3, 3 );
			ClearIfStale( ref OtherBody4, 4 );
			ClearIfStale( ref OtherBody5, 5 );
			ClearIfStale( ref OtherBody6, 6 );
			ClearIfStale( ref OtherBody7, 7 );
		}

		public byte GetStaleBits()
		{
			return (byte)~FreshnessFlags;
		}

		[MethodImpl( MethodImplOptions.AggressiveOptimization )]
		public bool HasFreshFlag( int index )
		{
			return (FreshnessFlags & 1 << index) != 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveOptimization )]
		public bool Has( int bodyId )
		{
			if ( OtherBody0 == bodyId ) return true;
			if ( OtherBody1 == bodyId ) return true;
			if ( OtherBody2 == bodyId ) return true;
			if ( OtherBody3 == bodyId ) return true;
			if ( OtherBody4 == bodyId ) return true;
			if ( OtherBody5 == bodyId ) return true;
			if ( OtherBody6 == bodyId ) return true;
			if ( OtherBody7 == bodyId ) return true;

			return false;
		}

		public bool HasFreshFlagForBody( int bodyId )
		{
			if ( OtherBody0 == bodyId && HasFreshFlag( 0 ) ) return true;
			if ( OtherBody1 == bodyId && HasFreshFlag( 1 ) ) return true;
			if ( OtherBody2 == bodyId && HasFreshFlag( 2 ) ) return true;
			if ( OtherBody3 == bodyId && HasFreshFlag( 3 ) ) return true;
			if ( OtherBody4 == bodyId && HasFreshFlag( 4 ) ) return true;
			if ( OtherBody5 == bodyId && HasFreshFlag( 5 ) ) return true;
			if ( OtherBody6 == bodyId && HasFreshFlag( 6 ) ) return true;
			if ( OtherBody7 == bodyId && HasFreshFlag( 7 ) ) return true;

			return false;
		}

		public int GetOtherBody( int index )
			=> index switch
			{
				0 => OtherBody0,
				1 => OtherBody1,
				2 => OtherBody2,
				3 => OtherBody3,
				4 => OtherBody4,
				5 => OtherBody5,
				6 => OtherBody6,
				_ => OtherBody7
			};

		public unsafe Span<int> AsSpan()
			=> new( Unsafe.AsPointer( ref OtherBody0 ), 8 );

		// TODO: Encode bits directly in the body IDs :)
		public int OtherBody0 = NoBody;
		public int OtherBody1 = NoBody;
		public int OtherBody2 = NoBody;
		public int OtherBody3 = NoBody;
		public int OtherBody4 = NoBody;
		public int OtherBody5 = NoBody;
		public int OtherBody6 = NoBody;
		public int OtherBody7 = NoBody;
		public byte FreshnessFlags;
	}

	public enum PendingCollisionEventType
	{
		Unknown = 0,
		StartedTouching,
		Touching,
		StoppedTouching
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct PendingCollisionEvent
	{
		/// <summary>
		/// Data associated with the collision, like position, normal etc.
		/// </summary>
		public CollisionData Data;

		/// <summary>
		/// The ID of the body that is sending the event.
		/// </summary>
		public int BodySender;

		/// <summary>
		/// The ID of the body that is receiving the event.
		/// </summary>
		public int BodyReceiver;

		/// <summary>
		/// What type of collision event this is (started, continued, ended).
		/// </summary>
		public PendingCollisionEventType Type;
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct PendingSeparationEvent
	{
		/// <summary>
		/// The ID of the body that is sending the event.
		/// </summary>
		public int BodySender;

		/// <summary>
		/// The ID of the body that is receiving the event.
		/// </summary>
		public int BodyReceiver;
	}

	public class CollisionEvents : IPhysicsSubsystem
	{
		private static TaggedLogger mLogger = new( "Collision" );

		public struct ContactWorkerCache
		{
			public ContactWorkerCache()
			{
			}

			public List<PendingCollisionEvent> PendingCollisions = new( 512 );
			public List<PendingSeparationEvent> PendingSeparations = new( 512 );
		}

		private Simulation mSimulation;
		private ContactWorkerCache[] mContactCaches = [];
		private CollidableProperty<ContactTracker> mContactTrackers;
		// TODO: entities should subscribe to different collision events instead of all of them having it
		private CollidableProperty<int> mSubscriberFlags;

		public ReadOnlySpan<ContactWorkerCache> Cache => mContactCaches;

		public void Init( Simulation simulation )
		{
			mContactTrackers = new( simulation );
			mSubscriberFlags = new( simulation );
			for ( int i = 0; i < mContactTrackers.BodyData.Length; i++ )
			{
				mContactTrackers.BodyData[i] = new();
			}

			simulation.Timestepper.BeforeCollisionDetection += PrepareBeforeCollisionDetection;
			simulation.Timestepper.CollisionsDetected += AnalyseCollisions;
			mSimulation = simulation;
		}

		/// <summary>
		/// Just allocates contact caches for worker threads.
		/// </summary>
		private void PrepareBeforeCollisionDetection( float dt, IThreadDispatcher? threadDispatcher )
		{
			int numThreads = threadDispatcher?.ThreadCount ?? 1;

			if ( mContactCaches.Length == 0 )
			{
				mContactCaches = new ContactWorkerCache[numThreads];
				foreach ( ref var cache in mContactCaches.AsSpan() )
				{
					cache = new();
				}
			}

			// Clear all events
			foreach ( ref var cache in mContactCaches.AsSpan() )
			{
				cache.PendingCollisions.Clear();
				cache.PendingSeparations.Clear();
			}

			// Resize if the number of threads magically changes
			if ( mContactCaches.Length < numThreads )
			{
				int oldSize = mContactCaches.Length;
				Array.Resize( ref mContactCaches, numThreads );
				for ( int i = oldSize; i < numThreads; i++ )
				{
					mContactCaches[i] = new();
				}
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveOptimization )]
		private Vector3 GetContactOffset<TManifold>( ref TManifold manifold, int i, bool swapOffsets )
			where TManifold : unmanaged, IContactManifold<TManifold>
		{
			Vector3 offsetToContact;
			Vector3 offsetToB;

			if ( manifold.Convex )
			{
				ref var convexManifold = ref Unsafe.As<TManifold, ConvexContactManifold>( ref manifold );
				offsetToContact = convexManifold.GetOffset( i );
				offsetToB = convexManifold.OffsetB;
			}
			else
			{
				ref var nonconvexManifold = ref Unsafe.As<TManifold, NonconvexContactManifold>( ref manifold );
				offsetToContact = nonconvexManifold.GetOffset( i );
				offsetToB = nonconvexManifold.OffsetB;
			}

			if ( swapOffsets )
			{
				return offsetToContact - offsetToB;
			}

			return offsetToContact;
		}

		/// <summary>
		/// Handles the accumulation of pending collision events.
		/// </summary>
		public void CollectCollisions<TManifold>( CollidableReference receiver, CollidableReference sender, bool swapOffset, ref TManifold manifold,
			int workerIndex )
			where TManifold : unmanaged, IContactManifold<TManifold>
		{
			// Triggers etc. are kinematic. Players and rigid bodies are dynamic.
			// Statics (world etc.) cannot receive events
			if ( receiver.Mobility == CollidableMobility.Static )
			{
				return;
			}

			ref var contactCache = ref mContactCaches[workerIndex];

			Span<Contact> contacts = stackalloc Contact[4];
			int numContacts = manifold.ExtractContacts( contacts );

			// Correct the offsets if we're going B to A
			if ( swapOffset )
			{
				for ( int i = 0; i < numContacts; i++ )
				{
					contacts[i].Offset = GetContactOffset( ref manifold, i, true );
				}
			}

			ref var tracker = ref mContactTrackers[receiver.BodyHandle];
			// CollisionTracker.NoBody = -1, so subtract 2 here
			int senderBodyId = sender.Mobility == CollidableMobility.Static ? -sender.StaticHandle.Value - 2 : sender.BodyHandle.Value;

			PendingCollisionEventType collisionType = tracker.Has( senderBodyId )
				? PendingCollisionEventType.Touching
				: PendingCollisionEventType.StartedTouching;

			mLogger.Log( $"Collision! Sender/receiver/type: {senderBodyId}/{receiver.BodyHandle.Value}/{collisionType}" );

			contactCache.PendingCollisions.Add( new()
			{
				BodyReceiver = receiver.BodyHandle.Value,
				BodySender = senderBodyId,
				Type = collisionType,
				Data = new( contacts[0], contacts[1], contacts[2], contacts[3] )
			} );
		}

		private int AnalyseTouches( ref ContactWorkerCache cache )
		{
			foreach ( ref var pending in cache.PendingCollisions.AsSpan() )
			{
				// Freshen up the reference to the body, as stale ones will be cleared at the end
				ref var tracker = ref mContactTrackers[new BodyHandle( pending.BodyReceiver )];
				tracker.Add( pending.BodySender );

				//mLogger.Log(
				//	$"Tracker state: {tracker.OtherBody0} {tracker.OtherBody1} {tracker.OtherBody2} {tracker.OtherBody3}" +
				//	$" {tracker.OtherBody4} {tracker.OtherBody5} {tracker.OtherBody6} {tracker.OtherBody7}" );
				//mLogger.Log( $"Freshness: {tracker.FreshnessFlags:b8}" );
			}

			return cache.PendingCollisions.Count;
		}

		private void AnalyseSeparationsWorker( int workerIndex )
		{
			(int start, int end) = Parallel.GetJobRange(
				items: mSimulation.Bodies.ActiveSet.Count,
				numThreads: mContactCaches.Length,
				workerIndex
			);

			// Correct it in case -1 is passed
			workerIndex = Math.Max( workerIndex, 0 );

			for ( int currentBody = start; currentBody < end; currentBody++ )
			{
				ref ContactTracker tracker = ref mContactTrackers[new BodyHandle( currentBody )];
				Span<int> otherBodies = tracker.AsSpan();

				for ( int other = 0; other < otherBodies.Length; other++ )
				{
					if ( otherBodies[other] == ContactTracker.NoBody )
					{
						continue;
					}

					// Basically, if the receiver has previously collided with this body, but is now stale,
					// they have effectively separated
					if ( !tracker.HasFreshFlagForBody( otherBodies[other] ) )
					{
						mContactCaches[workerIndex].PendingSeparations.Add( new()
						{
							BodyReceiver = currentBody,
							BodySender = otherBodies[other]
						} );

						//mLogger.Log( $"Separation! Sender/receiver/type: {otherBodies[other]}/{currentBody}/{PendingCollisionEventType.StoppedTouching}" );

						// Finally clear the body
						otherBodies[other] = ContactTracker.NoBody;
					}
				}
			}
		}

		/// <summary>
		/// Handles collision events when bodies are no longer touching.
		/// </summary>
		private unsafe void AnalyseCollisions( float dt, IThreadDispatcher? threadDispatcher )
		{
			int numItems = 0;

			//mLogger.Log( "AnalyseCollisions()" );

			// Step 1: Find out which contacts are new and update trackers
			foreach ( ref var cache in mContactCaches.AsSpan() )
			{
				numItems += AnalyseTouches( ref cache );
			}

			// Step 2: Find out which contacts have gone stale
			if ( threadDispatcher is null || numItems < 128 )
			{
				AnalyseSeparationsWorker( -1 );
			}
			else
			{
				threadDispatcher.DispatchWorkers( AnalyseSeparationsWorker );
			}

			// Step 3: Reset freshness flags so we can start anew on the next simulation tick
			foreach ( ref var tracker in mContactTrackers.BodyData )
			{
				tracker.FreshnessFlags = 0;
			}

			// TODO: What next?
		}

		// TODO: Dispose
	}
}
