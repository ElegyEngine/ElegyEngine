using BepuPhysics;
using BepuUtilities;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.PhysicsSystem.Subsystems
{
	public class DefaultGravity : IPhysicsSubsystem
	{
		public Vector3 Gravity;

		public void Init( Simulation simulation, Action<IContactFilter> registerFilter, Action<IContactModifier> registerModifier,
			Action<IIntegrator> registerIntegrator )
		{
			registerIntegrator( new GravityIntegrator { Gravity = Gravity } );
		}

		private struct GravityIntegrator : IIntegrator
		{
			private Vector3Wide gravityWideDt;
			public Vector3 Gravity;

			public void PrepareForIntegration( float dt )
			{
				// Precalculate gravity here
				gravityWideDt = Vector3Wide.Broadcast( Gravity * dt );
			}

			public void IntegrateVelocity(
				Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia,
				Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity )
			{
				velocity.Linear += gravityWideDt;
			}
		}
	}
}
