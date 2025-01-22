using BepuPhysics;
using BepuPhysics.Collidables;
using Elegy.Common.Maths;
using Game.Shared.Components;

namespace Game.Shared.Physics
{
	public class PhysicsBody
	{
		public PhysicsBody( PhysicsShape shape, BodyHandle dynamicHandle, bool needsConversion )
		{
			Shape = shape;
			BodyHandle = dynamicHandle;
			BodyReference = PhysicsWorld.Simulation.Bodies.GetBodyReference( dynamicHandle );
			StaticHandle = new( -1 );
			NeedsConversion = needsConversion;
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
		public bool NeedsConversion { get; }

		public Vector3 Position
		{
			get => BodyReference.Pose.Position;
			set => BodyReference.Pose.Position = value;
		}

		public Quaternion Orientation
		{
			get => NeedsConversion ? FromBepu( BodyReference.Pose.Orientation ) : BodyReference.Pose.Orientation;
			set => BodyReference.Pose.Orientation = NeedsConversion ? ToBepu( value ) : value;
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
