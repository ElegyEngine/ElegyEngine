// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Maths;

namespace Elegy.Assets.ModelData
{
	/// <summary>
	/// All possible vertex data that can be stored in EMFs.
	/// </summary>
	public struct Vertex
	{
		public Vertex()
		{

		}

		public Vector3 Position { get; set; } = Vector3.Zero;
		public Vector3 Normal { get; set; } = Vector3.UnitZ;
		public Vector3 Tangent { get; set; } = Vector3.UnitX;
		public Vector3 Bitangent { get; set; } = Vector3.UnitY;
		public Vector2 Uv1 { get; set; } = Vector2.Zero;
		public Vector2 Uv2 { get; set; } = Vector2.Zero;
		public Vector4 Color1 { get; set; } = Vector4.One;
		public Vector4 Color2 { get; set; } = Vector4.One;
		public Vector4I BoneIndices { get; set; } = Vector4I.One;
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
		public string Name { get; set; } = string.Empty;

		public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
	}
}
