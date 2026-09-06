// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;
using Game.Shared;
using Game.Shared.Components;
using Game.Shared.PhysicsSystem;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Server.Components
{
	[Component]
	[Requires<BodyKinematic>]
	public partial struct Trigger
	{
		private static TaggedLogger mLogger = new( "Trigger" );

		[Event]
		public void OnPostSpawn( Entity.PostSpawnEvent data )
		{
			// This is done in PostSpawn to make sure it happens *after*
			// BodyKinematic has finished setting itself up
			ref var body = ref data.Self.Ref<BodyKinematic>();

			body.SetLayer( CollisionLayer.Trigger );
		}

		[Event]
		public void Touch( Entity.TouchEvent data )
		{
			mLogger.Success( $"Touched by entity {data.Other.Id}" );

			// TODO: map logic, triggering...
			if ( data.Other.Has<Player>() )
			{
				OnPlayerEnter.Fire();
			}

			OnEnter.Fire();
		}

		[Property]
		public EntityOutput OnPlayerEnter { get; set; }

		[Property]
		public EntityOutput OnEnter { get; set; }
	}
}
