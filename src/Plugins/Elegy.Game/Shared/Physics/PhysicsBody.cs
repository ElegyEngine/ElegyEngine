using BepuPhysics;
using BepuPhysics.Collidables;
using Game.Shared.Components;

namespace Game.Shared.Physics
{
	public class PhysicsBody
	{
		public PhysicsBody( PhysicsShape shape, BodyHandle dynamicHandle )
		{
			Shape = shape;
			BodyHandle = dynamicHandle;
			BodyReference = PhysicsWorld.Simulation.Bodies.GetBodyReference( dynamicHandle );
			StaticHandle = new( -1 );
		}

		public PhysicsBody( PhysicsShape shape, StaticHandle staticHandle )
		{
			Shape = shape;
			StaticHandle = staticHandle;
			StaticReference = PhysicsWorld.Simulation.Statics.GetStaticReference( staticHandle );
		}
		
		public PhysicsShape Shape { get; }
		public BodyHandle BodyHandle { get; }
		public BodyReference BodyReference { get; }
		public StaticHandle StaticHandle { get; }
		public StaticReference StaticReference { get; }

		public bool IsStatic => StaticHandle.Value != -1;

		public Vector3 Position
		{
			get => BodyReference.Pose.Position;
			set => BodyReference.Pose.Position = value;
		}
		
		public Quaternion Orientation
		{
			get => BodyReference.Pose.Orientation;
			set => BodyReference.Pose.Orientation = value;
		}

		public Vector3 PositionStatic
		{
			get => StaticReference.Pose.Position;
			set => StaticReference.Pose.Position = value;
		}

		public Quaternion OrientationStatic
		{
			get => StaticReference.Pose.Orientation;
			set => StaticReference.Pose.Orientation = value;
		}
	}
}
