// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets.ElegyMapData
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
	}
}
