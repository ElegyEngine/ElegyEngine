// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets.ElegyMapData
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
	}
}
