// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Assets.MeshData
{
	/// <summary>
	/// Represents a joint between 2 bones.
	/// It is basically a transformation matrix with a name.
	/// </summary>
	public class BoneJoint
	{
		/// <summary></summary>
		public string Name { get; set; } = string.Empty;

		/// <summary></summary>
		public string Parent { get; set; } = string.Empty;

		/// <summary></summary>
		public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
	}
}
