// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Transform
	{
		public Transform()
		{
		}

		private Vector3 mPosition = Vector3.Zero;
		private Quaternion mOrientation = Quaternion.Identity;

		public bool TransformDirty { get; set; }

		public Vector3 Position
		{
			get => mPosition;
			set
			{
				mPosition = value;
				TransformDirty = true;
			}
		}

		public Quaternion Orientation
		{
			get => mOrientation;
			set
			{
				mOrientation = value;
				TransformDirty = true;
			}
		}
	}
}
