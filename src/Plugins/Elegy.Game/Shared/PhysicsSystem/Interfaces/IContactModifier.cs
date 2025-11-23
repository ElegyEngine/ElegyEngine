using BepuPhysics;
using BepuPhysics.CollisionDetection;

namespace Game.Shared.PhysicsSystem.Interfaces
{
	public interface IContactModifier
	{
		void Init( Simulation simulation )
		{
		}

		bool ConfigureContactManifold<TManifold>(
			int workerIndex, CollidablePair pair, ref TManifold manifold, ref PairMaterialProperties pairMaterial )
			where TManifold : unmanaged, IContactManifold<TManifold> => true;

		bool ConfigureContactManifold(
			int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold ) => true;

		void Dispose()
		{
		}
	}
}
