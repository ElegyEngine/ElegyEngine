// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets.MeshData;

namespace Elegy.Common.Assets.ElegyMapData
{
	/// <summary>
	/// Mesh used for occlusion culling.
	/// </summary>
	public class OccluderMesh
	{
		/// <summary>
		/// Vertex buffer.
		/// </summary>
		public List<Vector3> Positions { get; set; } = new();

		/// <summary>
		/// Index buffer.
		/// </summary>
		public List<int> Indices { get; set; } = new();

		/// <summary>
		/// Converts this into an Elegy render mesh.
		/// </summary>
		public Mesh ToMesh( string materialName = "null" )
			=> new()
			{
				Positions = Positions.ToArray(),
				Indices = Indices.Select( i => (uint)i ).ToArray(),
				MaterialName = materialName
			};
	}
}
