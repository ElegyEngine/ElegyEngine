﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets.ElegyMapData
{
	/// <summary>
	/// A collision submesh.
	/// </summary>
	public class CollisionMeshlet
	{
		/// <summary>
		/// Triplets of vector positions, forming collision triangles.
		/// </summary>
		public List<Vector3> Positions { get; set; } = new();

		/// <summary>
		/// Material that this mesh is associated with.
		/// </summary>
		public string MaterialName { get; set; } = string.Empty;
	}

	/// <summary>
	/// A collision mesh.
	/// </summary>
	public class CollisionMesh
	{
		/// <summary>
		/// Collision submeshes.
		/// </summary>
		public List<CollisionMeshlet> Meshlets { get; set; } = new();
	}
}
