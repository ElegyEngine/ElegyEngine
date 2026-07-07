using System.Runtime.CompilerServices;
using BepuUtilities;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using Game.Shared.PhysicsSystem.Interfaces;
using Game.Shared.PhysicsSystem.Subsystems;

namespace Game.Shared.PhysicsSystem
{
	public struct PhysicsCallbacks<TIntegrationConfig> : INarrowPhaseCallbacks, IPoseIntegratorCallbacks
		where TIntegrationConfig : struct, IIntegrationConfig
	{
		private readonly TIntegrationConfig mIntegrationConfig = new();
		private bool mInitialised;

		public required CharacterMovement Characters { get; init; }
		public required CollisionEvents Events { get; init; }
		public required CollisionFiltering<ClipMask> Filters { get; init; }
		public required DefaultGravity Gravity { get; init; }

		public PhysicsCallbacks()
		{
		}

		public void Initialize( Simulation simulation )
		{
			// As this implements both narrowphase and pose integrator callbacks, Initialize gets
			// called twice by BepuPhysics. This is, indeed, a hack :)
			if ( mInitialised )
			{
				return;
			}

			Characters.Init( simulation );
			Events.Init( simulation );
			Filters.Init( simulation );
			Gravity.Init( simulation );

			mInitialised = true;
		}

		#region Pose integrator callbacks

		public readonly AngularIntegrationMode AngularIntegrationMode => mIntegrationConfig.AngularIntegrationMode;
		public readonly bool AllowSubstepsForUnconstrainedBodies => mIntegrationConfig.AllowSubstepsForUnconstrainedBodies;
		public readonly bool IntegrateVelocityForKinematics => mIntegrationConfig.IntegrateVelocityForKinematics;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void PrepareForIntegration( float dt )
		{
			Gravity.PrepareForIntegration( dt );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void IntegrateVelocity( Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia,
			Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity )
		{
			Gravity.IntegrateVelocity( ref velocity );
		}

		#endregion

		#region Narrowphase callbacks

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin )
		{
			return Filters.CanCollide( a, b ) is not CollisionResponse.Discard;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidablePair pair, int childIndexA, int childIndexB )
		{
			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ConfigureContactManifold<TManifold>(
			int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial )
			where TManifold : unmanaged, IContactManifold<TManifold>
		{
			pairMaterial.FrictionCoefficient = 0.9f;
			pairMaterial.MaximumRecoveryVelocity = 2.0f;
			pairMaterial.SpringSettings = new SpringSettings( 60.0f, 0.1f );
			if ( manifold.Count is 0 )
			{
				return false;
			}

			var collisionResponse = Filters.CanCollide( pair.A, pair.B );
			if ( collisionResponse is CollisionResponse.Block )
			{
				Characters.TryReportContacts( pair, ref manifold, workerIndex, ref pairMaterial );
			}

			// Situations:
			// 1) A is dynamic/kinematic, B is static - remember, A is never static
			// 2) both A and B are dynamic/kinematic
			Events.CollectCollisions( pair.A, pair.B, false, ref manifold, workerIndex );
			Events.CollectCollisions( pair.B, pair.A, true, ref manifold, workerIndex );

			return collisionResponse is CollisionResponse.Block;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ConfigureContactManifold(
			int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold )
		{
			// TODO: Handle compound and mesh shapes. Their children have their own clip masks and the like
			//var shapeIndex = mSimulation.Bodies[pair.A.BodyHandle].Collidable.Shape;
			//if ( shapeIndex.Type is Compound.Id or BigCompound.Id or Mesh.Id )
			//{
			//	// Shape is a compound, look up child A and find out its physical properties (clip mask, friction, bounciness...)
			//	// ...
			//}

			return true;
		}

		#endregion

		public void Dispose()
		{
			Characters.Dispose();
		}
	}
}
