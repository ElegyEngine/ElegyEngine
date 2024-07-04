// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using SharpGLTF.Schema2;

using EngineMesh = Elegy.Common.Assets.MeshData.Mesh;

namespace Elegy.Common.Utilities
{
	public static class GltfHelpers
	{
		public static Vector3[] LoadPositions( Accessor value )
			=> value.AsVector3Array().ToArray();

		public static Vector3[] LoadNormals( Accessor value )
			=> value.AsVector3Array().ToArray();

		public static Vector4[] LoadTangents( Accessor value )
			=> value.AsVector4Array().ToArray();

		public static Vector2[] LoadUvs( Accessor value )
			=> value.AsVector2Array().ToArray();

		public static Vector4B[] LoadColours( Accessor value )
			=> EngineMesh.Transform( value.AsColorArray( 1.0f ),
				transform: ( vertex ) => new Vector4B( (byte)vertex.X, (byte)vertex.Y, (byte)vertex.Z, byte.MaxValue ) );

		public static Vector4B[] LoadJoints( Accessor value )
		{
			Vector4B[] list = new Vector4B[value.Count];
			ByteBuffer byteBuffer = new( value.SourceBufferView.Content.Array );
			for ( int i = 0; i < value.Count; i++ )
			{
				list[i] = byteBuffer.Read<Vector4B>();
			}

			return list;
		}

		public static Vector4[] LoadWeights( Accessor value )
			=> value.AsVector4Array().ToArray();

		public static uint[] LoadIndices( Accessor value )
			=> value.AsIndicesArray().ToArray();

		public static EngineMesh LoadMesh( MeshPrimitive primitive )
		{
			EngineMesh engineMesh = new();

			foreach ( var vertexAccessor in primitive.VertexAccessors )
			{
				switch ( vertexAccessor.Key )
				{
					case "POSITION": engineMesh.Positions = LoadPositions( vertexAccessor.Value ); break;
					case "NORMAL": engineMesh.Normals = LoadNormals( vertexAccessor.Value ); break;
					case "TANGENT": engineMesh.Tangents = LoadTangents( vertexAccessor.Value ); break;
					case "TEXCOORD_0": engineMesh.Uv0 = LoadUvs( vertexAccessor.Value ); break;
					case "TEXCOORD_1": engineMesh.Uv1 = LoadUvs( vertexAccessor.Value ); break;
					case "TEXCOORD_2": engineMesh.Uv2 = LoadUvs( vertexAccessor.Value ); break;
					case "TEXCOORD_3": engineMesh.Uv3 = LoadUvs( vertexAccessor.Value ); break;
					case "COLOR_0": engineMesh.Color0 = LoadColours( vertexAccessor.Value ); break;
					case "COLOR_1": engineMesh.Color1 = LoadColours( vertexAccessor.Value ); break;
					case "COLOR_2": engineMesh.Color2 = LoadColours( vertexAccessor.Value ); break;
					case "COLOR_3": engineMesh.Color3 = LoadColours( vertexAccessor.Value ); break;
					case "JOINTS_0": engineMesh.BoneIndices = LoadJoints( vertexAccessor.Value ); break;
					case "WEIGHTS_0": engineMesh.BoneWeights = LoadWeights( vertexAccessor.Value ); break;
				}
			}

			engineMesh.Indices = LoadIndices( primitive.IndexAccessor );
			engineMesh.MaterialName = primitive.Material.Name;

			return engineMesh;
		}
	}
}
