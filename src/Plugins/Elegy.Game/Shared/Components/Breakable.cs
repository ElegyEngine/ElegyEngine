// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Breakable
	{
		[Property]
		public int Health { get; set; }
		
		[Input]
		public void Break()
		{
			Health = -1;
		}
	}
}
