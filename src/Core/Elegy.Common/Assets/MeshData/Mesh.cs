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
		public Vector3[] Positions { get; set; } = Array.Empty<Vector3>();
		/// <summary> Vertex positions but 2D. </summary>
		public Vector2[] Positions2D { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex normals. </summary>
		public Vector3[] Normals { get; set; } = Array.Empty<Vector3>();
		/// <summary> Vertex normals but 2D. </summary>
		public Vector2[] Normals2D { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex tangents. </summary>
		public Vector4[] Tangents { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex texture coordinates, channel 0. </summary>
		public Vector2[] Uv0 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex texture coordinates, channel 1. </summary>
		public Vector2[] Uv1 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex texture coordinates, channel 2. </summary>
		public Vector2[] Uv2 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex texture coordinates, channel 3. </summary>
		public Vector2[] Uv3 { get; set; } = Array.Empty<Vector2>();
		/// <summary> Vertex colours, channel 0. </summary>
		public Vector4B[] Color0 { get; set; } = Array.Empty<Vector4B>();
		/// <summary> Vertex colours, channel 1. </summary>
		public Vector4B[] Color1 { get; set; } = Array.Empty<Vector4B>();
		/// <summary> Vertex colours, channel 2. </summary>
		public Vector4B[] Color2 { get; set; } = Array.Empty<Vector4B>();
		/// <summary> Vertex colours, channel 3. </summary>
		public Vector4B[] Color3 { get; set; } = Array.Empty<Vector4B>();
		/// <summary> Vertex bone indices. </summary>
		public Vector4B[] BoneIndices { get; set; } = Array.Empty<Vector4B>();
		/// <summary> Normalised vertex bone weights. </summary>
		public Vector4[] BoneWeights { get; set; } = Array.Empty<Vector4>();
		/// <summary> Vertex indices. </summary>
		public uint[] Indices { get; set; } = Array.Empty<uint>();

		/// <summary> Transforms a sequence of vertex data into another format, e.g. float3 into byte4. </summary>
		public static TDestination[] Transform<TSource, TDestination>( IList<TSource> source, Func<TSource, TDestination> transform )
			where TSource: unmanaged
			where TDestination: unmanaged
		{
			// Say no to LINQ !!!
			TDestination[] result = new TDestination[source.Count];
			for ( int i = 0; i < source.Count; i++ )
			{
				result[i] = transform( source[i] );
			}

			return result;
		}
	}
}
