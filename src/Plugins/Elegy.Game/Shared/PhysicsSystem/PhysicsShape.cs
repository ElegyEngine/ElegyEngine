// SPDX-FileCopyrightText: 2026 Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics;
using BepuPhysics.Collidables;

namespace Game.Shared.PhysicsSystem
{
	public class PhysicsShape
	{
		public BodyInertia Inertia { get; set; }
		public TypedIndex ShapeIndex { get; set; }
	}
}
