// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets.MeshData;

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

		/// <summary>
		/// Converts this into an Elegy render mesh.
		/// </summary>
		public Mesh ToMesh()
		{
			var uniquePositionIndices = Positions.ToVectorIndexDictionary();

			Mesh result = new()
			{
				MaterialName = MaterialName,
				Positions = uniquePositionIndices.Keys.ToArray(),
				Indices = Positions.Select( v =>
				{
					return (uint)uniquePositionIndices[v];
				} ).ToArray()
			};

			return result;
		}
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

		/// <summary>
		/// Converts this into a list of Elegy render meshes.
		/// </summary>
		public List<Mesh> ToMeshes()
			=> Meshlets.Select( m => m.ToMesh() ).ToList();
	}
}
