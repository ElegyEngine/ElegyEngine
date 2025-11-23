using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;

namespace Game.Shared.PhysicsSystem.Interfaces
{
	public interface IContactFilter
	{
		void Init( Simulation simulation )
		{
		}

		bool AllowContactGeneration( int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin ) => true;
		bool AllowContactGeneration( int workerIndex, CollidablePair pair, int childIndexA, int childIndexB ) => true;

		void Dispose()
		{
		}
	}
}
