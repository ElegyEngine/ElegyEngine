// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics.Collidables;
using Elegy.Common.Utilities;
using Elegy.ECS;
using Elegy.LogSystem;
using Game.Shared.PhysicsSystem;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.Components
{
	/// <summary>
	/// Kinematic physical body.
	/// </summary>
	[Component]
	[Requires<Transform>]
	public partial struct BodyKinematic : IBodyComponent
	{
		private static TaggedLogger mLogger = new( "BodyKinematic" );

		public BodyKinematic()
		{
		}

		public PhysicsShape Shape { get; private set; }

		public PhysicsBody BodyObject { get; private set; }

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

			//Shape = Physics.CreateShape( new Box( 0.5f, 0.5f, 0.5f ), mass: 1.0f );
			Shape = Physics.CreateMeshShape( CollisionModel.Data, mass: 1.0f );
			BodyObject = Physics.CreateKinematicBody( transform, Shape );

			this.SetOwner( data.Self );
			this.SetLayer( CollisionLayer.General );
		}

		[GroupEvent]
		public static void OnDebugDraw( Entity.DebugDrawEvent data, ref BodyKinematic bodyKinematic )
		{
			Physics.DebugDrawBody( bodyKinematic.BodyObject );
		}

		[GroupEvent]
		public static void UpdateTransforms( Entity.ServerTransformListenEvent data, ref BodyKinematic bodyKinematic, ref Transform transform )
		{
			if ( transform.Position == bodyKinematic.BodyObject.Position
			     && transform.Orientation == bodyKinematic.BodyObject.Orientation )
			{
				return;
			}

			transform.Position = bodyKinematic.BodyObject.Position;
			transform.Orientation = bodyKinematic.BodyObject.Orientation;
		}

		[Event]
		public void OnDespawn( Entity.DespawnEvent data )
		{
		}
	}
}
