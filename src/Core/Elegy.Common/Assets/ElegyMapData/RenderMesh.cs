// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

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
	}
}
