// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics.Collidables;
using Elegy.Common.Utilities;
using Elegy.ECS;
using Game.Shared.PhysicsSystem;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.Components
{
	/// <summary>
	/// Dynamic physical body.
	/// </summary>
	[Component]
	[Requires<Transform>]
	public partial struct Body : IBodyComponent
	{
		private static TaggedLogger mLogger = new( "Body" );

		public Body()
		{
		}

		public PhysicsShape Shape { get; private set; }

		public PhysicsBody BodyObject { get; private set; }

		[Property] public float Mass { get; set; } = 1.0f;

		[Property] public ModelProperty CollisionModel { get; set; }

		[Event]
		public void OnSpawn( SpawnEvent data )
		{
			ref var transform = ref data.Self.Ref<Transform>();

			if ( CollisionModel.Data is null )
			{
				mLogger.Error( $"Entity has invalid cmodel (at {transform.Position})" );
				return;
			}

			// TODO: create physics shape from actual collision models, not the visual ones
			Shape = Physics.CreateShape( new Box( 0.5f, 0.5f, 0.5f ), Mass );
			BodyObject = Physics.CreateBody( transform, Shape );

			this.SetOwner( data.Self );
			this.SetLayer( CollisionLayer.General );
		}

		[GroupEvent]
		public static void OnDebugDraw( DebugDrawEvent data, ref Body body )
		{
			Physics.DebugDrawBody( body.BodyObject );
		}

		[GroupEvent]
		public static void UpdateTransforms( ServerTransformListenEvent data, ref Body body, ref Transform transform )
		{
			if ( !body.BodyObject.BodyReference.Awake )
			{
				return;
			}

			transform.Position = body.BodyObject.Position;
			transform.Orientation = body.BodyObject.Orientation;
		}

		[Event]
		public void OnDespawn( DespawnEvent data )
		{
		}
	}
}
