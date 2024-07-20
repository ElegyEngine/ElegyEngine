// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets.MeshData;

namespace Elegy.Common.Assets.ElegyMapData
{
	/// <summary>
	/// A render mesh is a collection of renderable surfaces.
	/// Definitely read more about <seealso cref="RenderSurface"/>.
	/// </summary>
	public class RenderMesh
	{
		/// <summary>
		/// The render surfaces of this mesh.
		/// </summary>
		public List<RenderSurface> Surfaces { get; set; } = new();

		/// <summary>
		/// Converts this into a list of Elegy render meshes.
		/// </summary>
		public List<Mesh> ToMeshes()
			=> Surfaces.Select( s => s.ToMesh() ).ToList();
	}
}
