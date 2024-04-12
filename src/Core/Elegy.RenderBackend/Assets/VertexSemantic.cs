// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderBackend.Assets
{
	/// <summary>
	/// The meaning behind a vertex layout element.
	/// </summary>
	public enum VertexSemantic
	{
		/// <summary> 3D vertex positions. </summary>
		Position,
		/// <summary> Vertex normals. </summary>
		Normal,
		/// <summary> Vertex tangents. </summary>
		Tangent,
		/// <summary> Vertex UVs. </summary>
		Uv,
		/// <summary> Vertex colours. </summary>
		Colour,
		/// <summary> Vertex bone weights. </summary>
		BoneWeight,
		/// <summary> Vertex bone indices. </summary>
		BoneIndex
	}
}
