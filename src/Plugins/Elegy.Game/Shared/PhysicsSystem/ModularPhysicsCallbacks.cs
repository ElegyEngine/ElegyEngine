using System.Runtime.CompilerServices;
using BepuUtilities;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.PhysicsSystem
{
	// TODO: Maybe codegen a more optimised version?
	// [PhysicsCallbacks<CharacterMovementSystem, DefaultGravitySystem, ...>]
	// public partial struct ModularPhysicsCallbacks {}
	public struct ModularPhysicsCallbacks<TIntegrationConfig> : INarrowPhaseCallbacks, IPoseIntegratorCallbacks
		where TIntegrationConfig : struct, IIntegrationConfig
	{
		private TIntegrationConfig mIntegrationConfig = new();
		private List<IContactFilter> mContactFilters = new();
		private List<IContactModifier> mContactModifiers = new();
		private List<IIntegrator> mIntegratorCallbacks = new();
		private List<IPhysicsSubsystem> mSystems = new();
		private bool mInitialised;

		public ModularPhysicsCallbacks()
		{
		}

		#region Pose integrator callbacks

		public readonly AngularIntegrationMode AngularIntegrationMode => mIntegrationConfig.AngularIntegrationMode;
		public readonly bool AllowSubstepsForUnconstrainedBodies => mIntegrationConfig.AllowSubstepsForUnconstrainedBodies;
		public readonly bool IntegrateVelocityForKinematics => mIntegrationConfig.IntegrateVelocityForKinematics;

		public void Initialize( Simulation simulation )
		{
			// As this implements both narrowphase and pose integrator callbacks, Initialize gets
			// called twice by BepuPhysics. This is, indeed, a hack :)
			if ( mInitialised )
			{
				return;
			}

			foreach ( IPhysicsSubsystem system in mSystems.AsSpan() )
			{
				system.Init( simulation, RegisterFilter, RegisterModifier, RegisterIntegrator );
			}
			foreach ( IContactFilter callback in mContactFilters.AsSpan() )
			{
				callback.Init( simulation );
			}
			foreach ( IContactModifier callback in mContactModifiers.AsSpan() )
			{
				callback.Init( simulation );
			}
			foreach ( IIntegrator callback in mIntegratorCallbacks.AsSpan() )
			{
				callback.Init( simulation );
			}

			mInitialised = true;
		}

		public T GetSystem<T>()
			where T : class, IPhysicsSubsystem
		{
			var span = mSystems.AsSpan();
			for ( int i = 0; i < span.Length; i++ )
			{
				if ( span[i] is T )
				{
					return (T)span[i];
				}
			}

			throw new KeyNotFoundException( $"Cannot find {typeof(T)} among physics subsystems" );
		}

		public void Register( IPhysicsSubsystem system )
		{
			mSystems.Add( system );
		}

		public void RegisterFilter( IContactFilter filter )
		{
			mContactFilters.Add( filter );
		}

		public void RegisterModifier( IContactModifier modifier )
		{
			mContactModifiers.Add( modifier );
		}

		public void RegisterIntegrator( IIntegrator callback )
		{
			mIntegratorCallbacks.Add( callback );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void PrepareForIntegration( float dt )
		{
			foreach ( IIntegrator callback in mIntegratorCallbacks.AsSpan() )
			{
				callback.PrepareForIntegration( dt );
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void IntegrateVelocity( Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia,
			Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity )
		{
			foreach ( IIntegrator callback in mIntegratorCallbacks.AsSpan() )
			{
				callback.IntegrateVelocity( bodyIndices, position, orientation, localInertia, integrationMask, workerIndex, dt, ref velocity );
			}
		}

		#endregion

		#region Narrowphase callbacks

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin )
		{
			foreach ( IContactFilter callback in mContactFilters.AsSpan() )
			{
				// The moment a subsystem thinks two objects should pass through each other, that's it
				if ( !callback.AllowContactGeneration( workerIndex, a, b, ref speculativeMargin ) )
				{
					return false;
				}
			}

			return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool AllowContactGeneration( int workerIndex, CollidablePair pair, int childIndexA, int childIndexB )
		{
			foreach ( IContactFilter callback in mContactFilters.AsSpan() )
			{
				// The moment a subsystem thinks two objects should pass through each other, that's it
				if ( !callback.AllowContactGeneration( workerIndex, pair, childIndexA, childIndexB ) )
				{
					return false;
				}
			}

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

			foreach ( IContactModifier callback in mContactModifiers.AsSpan() )
			{
				// The moment a subsystem thinks two objects should pass through each other, that's it
				if ( !callback.ConfigureContactManifold( workerIndex, pair, ref manifold, ref pairMaterial ) )
				{
					return false;
				}
			}

			return true;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ConfigureContactManifold(
			int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold )
		{
			foreach ( IContactModifier callback in mContactModifiers.AsSpan() )
			{
				// The moment a subsystem thinks two objects should pass through each other, that's it
				if ( !callback.ConfigureContactManifold( workerIndex, pair, childIndexA, childIndexB, ref manifold ) )
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		public void Dispose()
		{
			foreach ( IContactFilter callback in mContactFilters.AsSpan() )
			{
				callback.Dispose();
			}
			foreach ( IContactModifier callback in mContactModifiers.AsSpan() )
			{
				callback.Dispose();
			}
			foreach ( IIntegrator callback in mIntegratorCallbacks.AsSpan() )
			{
				callback.Dispose();
			}
			foreach ( IPhysicsSubsystem system in mSystems.AsSpan() )
			{
				system.Dispose();
			}
		}
	}
}
