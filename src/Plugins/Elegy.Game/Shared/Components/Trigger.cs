// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	//[Requires<Collider>] // we don't have those yet
	public partial struct Trigger
	{
		[Event]
		public void Touch( Entity.TouchEvent data )
		{
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
