// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace PhysBench
{
	public struct TestContact
	{
		public Vector4 PositionAndDepth;
		public Vector3 Normal;
		public int FeatureId;
	}

	public struct NarrowphaseCallbacks : INarrowPhaseCallbacks
	{
		private Simulation mSimulation;

		public event Action<int, TestContact>? OnCollision;

		/// <summary>
		/// Performs initialisation logic.
		/// </summary>
		public void Initialize( Simulation simulation )
		{
			mSimulation = simulation;
		}

		/// <summary>
		/// Chooses whether to allow contact generation for two overlapping collidable objects.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidableReference a, CollidableReference b,
			ref float speculativeMargin )
		{
			return a.Mobility is CollidableMobility.Dynamic or CollidableMobility.Kinematic 
			       || b.Mobility is CollidableMobility.Dynamic or CollidableMobility.Kinematic;
		}

		/// <summary>
		/// Chooses whether to allow contact generation for two overlapping collidable objects, in a compound-including pair.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidablePair pair, int childIndexA, int childIndexB )
		{
			// This is similar to the top level broad phase callback above.
			// It's called by the narrow phase before generating subpairs between children in parent shapes. 
			// This only gets called in pairs that involve at least one shape type that can contain multiple children, like a Compound.
			return true;
		}

		private Vector3 GetCollidablePosition( CollidableReference c )
			=> c.Mobility switch
			{
				CollidableMobility.Dynamic or CollidableMobility.Kinematic
					=> mSimulation.Bodies[c.BodyHandle].Pose.Position,

				_ => mSimulation.Statics[c.StaticHandle].Pose.Position
			};

		/// <summary>
		/// Provides a notification that a manifold has been created for a pair.
		/// Offers an opportunity to change the manifold's details. 
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ConfigureContactManifold<TManifold>(
			int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial )
			where TManifold : unmanaged, IContactManifold<TManifold>
		{
			for ( int i = 0; i < manifold.Count; i++ )
			{
				Vector3 position = GetCollidablePosition( pair.A ) + manifold.GetOffset( i );

				OnCollision?.Invoke( workerIndex, new()
				{
					PositionAndDepth = new( position, manifold.GetDepth( i ) ),
					Normal = manifold.GetNormal( i ),
					FeatureId = manifold.GetFeatureId( i )
				} );
			}

			pairMaterial.FrictionCoefficient = 0.9f;
			pairMaterial.MaximumRecoveryVelocity = 2.0f;
			pairMaterial.SpringSettings = new SpringSettings( 60.0f, 0.1f );
			return true;
		}

		/// <summary>
		/// Provides a notification that a manifold has been created between the children of two collidables in a compound-including pair.
		/// Offers an opportunity to change the manifold's details. 
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ConfigureContactManifold(
			int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold )
		{
			return true;
		}

		public void Dispose()
		{
		}
	}
}
