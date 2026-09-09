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
		public void OnPostSpawn( PostSpawnEvent data )
		{
			// This is done in PostSpawn to make sure it happens *after*
			// BodyKinematic has finished setting itself up
			ref var body = ref data.Self.Ref<BodyKinematic>();
			body.MakeImmovable();
			body.SetLayer( CollisionLayer.Trigger );
		}

		[Event]
		public void Touch( TouchEvent data )
		{
			mLogger.Success( $"Touched by entity {data.Other.Ref<EntitySlot>().Id}" );

			if ( data.Other.Has<Player>() )
			{
				OnPlayerEnter.Fire();
			}

			OnEnter.Fire();
		}

		[Event]
		public void TouchEnded( TouchEndEvent data )
		{
			mLogger.Success( $"Touch with entity {data.Other.Ref<EntitySlot>().Id} ended" );

			if ( data.Other.Has<Player>() )
			{
				OnPlayerLeave.Fire();
			}

			OnLeave.Fire();
		}

		[Property]
		public EntityOutput OnPlayerEnter { get; set; }

		[Property]
		public EntityOutput OnEnter { get; set; }

		[Property]
		public EntityOutput OnPlayerLeave { get; set; }

		[Property]
		public EntityOutput OnLeave { get; set; }
	}
}
