// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;
using Elegy.ConsoleSystem;
using Game.Shared.Physics;

namespace Game.Shared.Components
{
	/// <summary>
	/// A physical, rigid body. Can be a static world collider, but can also
	/// be a basic dynamic or kinematic body.
	/// </summary>
	[Component]
	[Requires<Transform>]
	public partial struct Body
	{
		private static TaggedLogger mLogger = new( "Body" );

		public Body()
		{
		}

		public PhysicsShape Shape { get; private set; }

		public PhysicsBody BodyObject { get; private set; }

		[Property] public bool Static { get; set; } = true;

		[Property] public float Mass { get; set; } = 1.0f;

		[Property] public ModelProperty CollisionModel { get; set; }

		[Event]
		public void OnSpawn( Entity.SpawnEvent data )
		{
			ref var transform = ref data.Self.Ref<Transform>();

			if ( CollisionModel.Data is null )
			{
				mLogger.Error( $"Entity has invalid cmodel (at {transform.Position})" );
				return;
			}

			// TODO: create physics shape from actual collision models, not the visual ones
			Shape = PhysicsWorld.CreateMeshShape( CollisionModel.Data, Mass );
			BodyObject = Static
				? PhysicsWorld.CreateStaticBody( transform, Shape )
				: PhysicsWorld.CreateBody( transform, Shape );
		}

		[GroupEvent]
		public static void OnDebugDraw( Entity.DebugDrawEvent data, ref Body body )
		{
			PhysicsWorld.DebugDrawBody( body.BodyObject );
		}

		[GroupEvent]
		public static void UpdateTransforms( Entity.ServerTransformListenEvent data, ref Body body, ref Transform transform )
		{
			// Static bodies don't move via physics simulation, so
			if ( body.Static )
			{
				return;
			}

			if ( !body.BodyObject.BodyReference.Awake )
			{
				return;
			}

			transform.Position = body.BodyObject.Position;
			transform.Orientation = body.BodyObject.Orientation;
		}

		[Event]
		public void OnDespawn( Entity.DespawnEvent data )
		{
		}
	}
}
