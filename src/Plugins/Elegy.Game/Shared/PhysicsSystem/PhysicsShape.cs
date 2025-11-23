using BepuPhysics;
using BepuPhysics.Collidables;

namespace Game.Shared.PhysicsSystem
{
	public class PhysicsShape
	{
		public BodyInertia Inertia { get; set; }
		public TypedIndex ShapeIndex { get; set; }
	}
}
