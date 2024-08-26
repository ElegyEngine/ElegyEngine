// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[GameComponent]
	public struct Player
	{
		[EntityEvent<Entity.SpawnEvent>]
		public void Spawn()
		{

		}
	}
}
