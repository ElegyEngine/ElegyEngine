// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Assets.MeshData
{
	/// <summary>
	/// All possible vertex data that can be stored in EMFs.
	/// </summary>
	public struct Vertex
	{
		/// <summary></summary>
		public Vertex()
		{

		}

		/// <summary></summary>
		public Vector3 Position { get; set; } = Vector3.Zero;
		/// <summary></summary>
		public Vector3 Normal { get; set; } = Vector3.UnitZ;
		/// <summary></summary>
		public Vector3 Tangent { get; set; } = Vector3.UnitX;
		/// <summary></summary>
		public Vector3 Bitangent { get; set; } = Vector3.UnitY;
		/// <summary></summary>
		public Vector2 Uv1 { get; set; } = Vector2.Zero;
		/// <summary></summary>
		public Vector2 Uv2 { get; set; } = Vector2.Zero;
		/// <summary></summary>
		public Vector4 Color1 { get; set; } = Vector4.One;
		/// <summary></summary>
		public Vector4 Color2 { get; set; } = Vector4.One;
		/// <summary></summary>
		public Vector4I BoneIndices { get; set; } = Vector4I.One;
		/// <summary></summary>
		public Vector4 BoneWeights { get; set; } = Vector4.One;
	}

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

		/// <summary>
		/// Vertex data.
		/// </summary>
		public List<Vertex> Vertices { get; set; } = new();

		/// <summary>
		/// Vertex indices.
		/// </summary>
		public List<int> Indices { get; set; } = new();
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
