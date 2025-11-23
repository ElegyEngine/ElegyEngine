using BepuPhysics;
using BepuUtilities;

namespace Game.Shared.PhysicsSystem.Interfaces
{
	public interface IIntegrator
	{
		void Init( Simulation simulation )
		{
		}

		void PrepareForIntegration( float dt )
		{
		}

		void IntegrateVelocity(
			Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia,
			Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity )
		{
		}

		void Dispose()
		{
		}
	}
}
