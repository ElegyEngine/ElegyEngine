// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Worldspawn
	{
		public Worldspawn() { }

		[Property]
		public string Name { get; set; } = string.Empty;
	}
}
