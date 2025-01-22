// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace Game.Shared.Physics
{
	public struct NarrowphaseCallbacks : INarrowPhaseCallbacks
	{
		/// <summary>
		/// Performs initialisation logic.
		/// </summary>
		public void Initialize( Simulation simulation )
		{
		}

		/// <summary>
		/// Chooses whether to allow contact generation for two overlapping collidable objects.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin )
		{
			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
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

		/// <summary>
		/// Provides a notification that a manifold has been created for a pair.
		/// Offers an opportunity to change the manifold's details. 
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ConfigureContactManifold<TManifold>(
			int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial )
			where TManifold : unmanaged, IContactManifold<TManifold>
		{
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
