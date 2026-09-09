// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;
using Game.Shared.PhysicsSystem;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.Components
{
	/// <summary>
	/// Static physical body.
	/// </summary>
	[Component]
	[Requires<Transform>]
	public partial struct BodyStatic : IBodyComponent
	{
		private static TaggedLogger mLogger = new( "BodyStatic" );

		public BodyStatic()
		{
		}

		public PhysicsShape Shape { get; private set; }

		public PhysicsBody BodyObject { get; private set; }

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
			Shape = Physics.CreateMeshShape( CollisionModel.Data );
			BodyObject = Physics.CreateStaticBody( transform, Shape );

			this.SetOwner( data.Self );
			this.SetLayer( CollisionLayer.General );
		}

		[GroupEvent]
		public static void OnDebugDraw( DebugDrawEvent data, ref BodyStatic body )
		{
			Physics.DebugDrawBody( body.BodyObject );
		}

		[Event]
		public void OnDespawn( DespawnEvent data )
		{
		}
	}
}
