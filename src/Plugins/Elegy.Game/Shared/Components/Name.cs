// SPDX-FileCopyrightText: 2026 Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Name
	{
		[Property]
		public string Targetname { get; set; }
	}
}
