using BepuPhysics;
using BepuPhysics.Collidables;
using Elegy.Common.Maths;
using Game.Shared.Components;

namespace Game.Shared.PhysicsSystem
{
	// TODO: PhysicsBody and PhysicsShape should be handles
	public class PhysicsBody
	{
		public PhysicsBody( PhysicsShape shape, BodyHandle dynamicHandle )
		{
			Shape = shape;
			BodyReference = Physics.Simulation.Bodies.GetBodyReference( dynamicHandle );
			StaticReference = new( new( -1 ), Physics.Simulation.Statics );
		}

		public PhysicsBody( PhysicsShape shape, StaticHandle staticHandle )
		{
			Shape = shape;
			BodyReference = new( new( -1 ), Physics.Simulation.Bodies );
			StaticReference = Physics.Simulation.Statics.GetStaticReference( staticHandle );
		}

		public PhysicsShape Shape { get; }
		public BodyHandle BodyHandle => BodyReference.Handle;
		public BodyReference BodyReference { get; }
		public StaticHandle StaticHandle => StaticReference.Handle;
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

		public static readonly Quaternion FromBepuQuaternion = Coords.QuatFromAxisAngle( Vector3.UnitX, -float.Pi / 2.0f );
		public static readonly Quaternion ToBepuQuaternion = Coords.QuatFromAxisAngle( Vector3.UnitX, float.Pi / 2.0f );

		public static Quaternion FromBepu( Quaternion orientation )
			=> orientation * FromBepuQuaternion;

		public static Quaternion ToBepu( Quaternion orientation )
			=> orientation * ToBepuQuaternion;
	}
}
