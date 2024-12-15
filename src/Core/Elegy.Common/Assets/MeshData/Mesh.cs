// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Assets.MeshData
{
	/// <summary>
	/// Flags which describe the vertex layout of a mesh.
	/// </summary>
	[Flags]
	public enum MeshVertexFlags
	{
		Positions = 1,
		Positions2D = 2,
		Normals = 4,
		Normals2D = 8,
		Tangents = 16,
		Uv0 = 32,
		Uv1 = 64,
		Uv2 = 128,
		Uv3 = 256,
		Color0 = 512,
		Color1 = 1024,
		Color2 = 2048,
		Color3 = 4096,
		BoneIndices = 8192,
		BoneWeights = 16384
	}

	/// <summary>
	/// Represents a particular part of a model with a material assigned to it.
	/// </summary>
	public class Mesh
	{
		/// <summary>
		/// Name of the model, usually the file name + surface ID.
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Material name, is used by the renderer system to look for materials.
		/// </summary>
		public string MaterialName { get; set; } = string.Empty;

		/// <summary> Number of dynamic vertices. -1 if it's not a dynamic mesh. </summary>
		public int NumDynamicVertices { get; set; } = -1;

		/// <summary> Number of dynamic indices. -1 if it's not a dynamic mesh. </summary>
		public int NumDynamicIndices { get; set; } = -1;

		/// <summary> Whether this is a dynamic mesh. </summary>
		public bool IsDynamic => NumDynamicVertices is not -1;

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

		/// <summary> Gets the vertex layout of the mesh, in form of flags. </summary>
		public MeshVertexFlags GetVertexFlags()
		{
			MeshVertexFlags result = 0;

			void Check<T>( T[] array, MeshVertexFlags flag )
			{
				if ( array.Length != 0 )
				{
					result |= flag;
				}
			}

			Check( Positions, MeshVertexFlags.Positions );
			Check( Positions2D, MeshVertexFlags.Positions2D );
			Check( Normals, MeshVertexFlags.Normals );
			Check( Normals2D, MeshVertexFlags.Normals2D );
			Check( Tangents, MeshVertexFlags.Tangents );
			Check( Uv0, MeshVertexFlags.Uv0 );
			Check( Uv1, MeshVertexFlags.Uv1 );
			Check( Uv2, MeshVertexFlags.Uv2 );
			Check( Uv3, MeshVertexFlags.Uv3 );
			Check( Color0, MeshVertexFlags.Color0 );
			Check( Color1, MeshVertexFlags.Color1 );
			Check( Color2, MeshVertexFlags.Color2 );
			Check( Color3, MeshVertexFlags.Color3 );
			Check( BoneIndices, MeshVertexFlags.BoneIndices );
			Check( BoneWeights, MeshVertexFlags.BoneWeights );

			return result;
		}

		/// <summary> Transforms a sequence of vertex data into another format, e.g. float3 into byte4. </summary>
		public static TDestination[] Transform<TSource, TDestination>( IList<TSource> source, Func<TSource, TDestination> transform )
			where TSource : unmanaged
			where TDestination : unmanaged
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
