using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;

namespace Game.Shared.PhysicsSystem.Interfaces
{
	/// <summary>
	/// A system that extends the functionality of the physics engine.
	/// Refer to some existing implementations, like character movement,
	/// event reporting etc.
	/// </summary>
	public interface IPhysicsSubsystem
	{
		/// <summary>
		/// Initial setup before the physics simulation has started. This is where
		/// callbacks will be registered.
		/// </summary>
		void Init( Simulation simulation );
	}
}
