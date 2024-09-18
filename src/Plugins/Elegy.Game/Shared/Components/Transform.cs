// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Transform
	{
		public Vector3 Position { get; set; }
		public Quaternion Orientation { get; set; }
	}
}
