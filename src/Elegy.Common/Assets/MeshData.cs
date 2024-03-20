// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Assets.MeshData
{
	/// <summary>
	/// Represents a particular part of a model with a material assigned to it.
	/// </summary>
	public class Mesh
	{
		/// <summary>
		/// Name of the model, usually the file name.
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Material name, is used by the renderer system to look for materials.
		/// </summary>
		public string MaterialName { get; set; } = string.Empty;

		/// <summary> Vertex positions. </summary>
		public IList<Vector3> Positions { get; set; } = Array.Empty<Vector3>();
		/// <summary> Vertex normals. </summary>
		public IList<Vector3> Normals { get; set; } = Array.Empty<Vector3>();
		/// <summary> Vertex tangents. </summary>
		public IList<Vector3> Tangent { get; set; } = Array.Empty<Vector3>();
		/// <summary> Vertex bitangents, usually derived from normal and tangent. </summary>
		public IList<Vector3> Bitangent { get; set; } = Array.Empty<Vector3>();
		/// <summary> Vertex texture coordinates, channel 0. </summary>
		public IList<Vector2> Uv0 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex texture coordinates, channel 1. </summary>
		public IList<Vector2> Uv1 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex texture coordinates, channel 2. </summary>
		public IList<Vector2> Uv2 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex texture coordinates, channel 3. </summary>
		public IList<Vector2> Uv3 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex colours, channel 0. </summary>
		public IList<Vector4> Color0 { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex colours, channel 1. </summary>
		public IList<Vector4> Color1 { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex colours, channel 2. </summary>
		public IList<Vector4> Color2 { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex colours, channel 3. </summary>
		public IList<Vector4> Color3 { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex bone indices. </summary>
		public IList<Vector4B> BoneIndices { get; set; } = Array.Empty<Vector4B>();
		/// <summary> Normalised vertex bone weights. </summary>
		public IList<Vector4> BoneWeights { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex indices. </summary>
		public IList<uint> Indices { get; set; } = Array.Empty<uint>();
	}

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
