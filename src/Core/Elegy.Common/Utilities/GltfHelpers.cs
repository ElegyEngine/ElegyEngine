// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets.MeshData;
using Elegy.Common.Maths;
using EngineMesh = Elegy.Common.Assets.MeshData.Mesh;
using GltfMesh = SharpGLTF.Schema2.Mesh;

using SharpGLTF.Schema2;
using Buffer = SharpGLTF.Schema2.Buffer;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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

		public static string GetAccessorNameForVertexFlag( MeshVertexFlags? flag )
			=> flag switch
			{
				MeshVertexFlags.Positions or
				MeshVertexFlags.Positions2D => "POSITION",
				MeshVertexFlags.Normals or
				MeshVertexFlags.Normals2D => "NORMAL",
				MeshVertexFlags.Tangents => "TANGENT",
				MeshVertexFlags.Uv0 => "TEXCOORD_0",
				MeshVertexFlags.Uv1 => "TEXCOORD_1",
				MeshVertexFlags.Uv2 => "TEXCOORD_2",
				MeshVertexFlags.Uv3 => "TEXCOORD_3",
				MeshVertexFlags.Color0 => "COLOR_0",
				MeshVertexFlags.Color1 => "COLOR_1",
				MeshVertexFlags.Color2 => "COLOR_2",
				MeshVertexFlags.Color3 => "COLOR_3",
				MeshVertexFlags.BoneIndices => "JOINTS_0",
				MeshVertexFlags.BoneWeights => "WEIGHTS_0",
				null => "INDICES",
				_ => throw new NotSupportedException()
			};

		public static Accessor CreateBufferAndAccessorFromData<T>( ModelRoot root, string modelName, MeshVertexFlags? flag, T[] values )
			where T: unmanaged
		{
			string extension = GetAccessorNameForVertexFlag( flag );

			(DimensionType dimensions, EncodingType encoding, bool normalised) = flag switch
			{
				MeshVertexFlags.Positions => (DimensionType.VEC3, EncodingType.FLOAT, false),
				MeshVertexFlags.Positions2D => (DimensionType.VEC2, EncodingType.FLOAT, false),
				MeshVertexFlags.Normals => (DimensionType.VEC3, EncodingType.FLOAT, false),
				MeshVertexFlags.Normals2D => (DimensionType.VEC2, EncodingType.FLOAT, false),
				MeshVertexFlags.Tangents => (DimensionType.VEC4, EncodingType.FLOAT, false),
				MeshVertexFlags.Uv0 => (DimensionType.VEC2, EncodingType.FLOAT, false),
				MeshVertexFlags.Uv1 => (DimensionType.VEC2, EncodingType.FLOAT, false),
				MeshVertexFlags.Uv2 => (DimensionType.VEC2, EncodingType.FLOAT, false),
				MeshVertexFlags.Uv3 => (DimensionType.VEC2, EncodingType.FLOAT, false),
				MeshVertexFlags.Color0 => (DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, true),
				MeshVertexFlags.Color1 => (DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, true),
				MeshVertexFlags.Color2 => (DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, true),
				MeshVertexFlags.Color3 => (DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, true),
				MeshVertexFlags.BoneIndices => (DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, false),
				MeshVertexFlags.BoneWeights => (DimensionType.VEC4, EncodingType.FLOAT, false),
				null => (DimensionType.SCALAR, EncodingType.UNSIGNED_INT, false),
				_ => throw new NotSupportedException()
			};

			var valuesBytes = MemoryMarshal.AsBytes( values.AsSpan() );
			Buffer buffer = root.CreateBuffer( valuesBytes.Length );
			buffer.Name = $"{modelName}_{extension}.buf";

			valuesBytes.CopyTo( buffer.Content.AsSpan() );

			// For some reason, SharpGLTF (or GLTF itself?) requires the index buffer
			// to be of stride 0? Fair enough I guess
			BufferView bufferView = root.UseBufferView( buffer, 0, valuesBytes.Length,
				flag is null ? 0 : Unsafe.SizeOf<T>() );

			bufferView.Name = $"{buffer.Name}v";

			Accessor result = root.CreateAccessor( $"{modelName}_{extension}" );
			if ( flag is null )
			{
				result.SetIndexData( bufferView, 0, values.Length, IndexEncodingType.UNSIGNED_INT );
			}
			else
			{
				result.SetVertexData( bufferView, 0, values.Length, dimensions, encoding, normalised );
			}

			return result;
		}

		public static GltfMesh WriteMesh( ModelRoot root, string name, IReadOnlyList<EngineMesh> meshes )
		{
			GltfMesh result = root.CreateMesh( name );
			Scene rootScene = root.UseScene( 0 );

			Material useMaterial( string name )
			{
				foreach ( var material in root.LogicalMaterials )
				{
					if ( material.Name == name )
					{
						return material;
					}
				}

				return root.CreateMaterial( name );
			}

			foreach ( var mesh in meshes )
			{
				var primitive = result.CreatePrimitive();

				// If this ever causes perf/memory issues, we manually unroll this thing
				void tryAppendAccessor<T>( MeshVertexFlags flag, T[] values )
					where T: unmanaged
				{
					if ( mesh.GetVertexFlags().HasFlag( flag ) )
					{
						primitive.SetVertexAccessor(
							GetAccessorNameForVertexFlag( flag ),
							CreateBufferAndAccessorFromData( root, name, flag, values ) );
					}
				}

				tryAppendAccessor( MeshVertexFlags.Positions, mesh.Positions );
				tryAppendAccessor( MeshVertexFlags.Positions2D, mesh.Positions2D );
				tryAppendAccessor( MeshVertexFlags.Normals, mesh.Normals );
				tryAppendAccessor( MeshVertexFlags.Normals2D, mesh.Normals2D );
				tryAppendAccessor( MeshVertexFlags.Tangents, mesh.Tangents );
				tryAppendAccessor( MeshVertexFlags.Uv0, mesh.Uv0 );
				tryAppendAccessor( MeshVertexFlags.Uv1, mesh.Uv1 );
				tryAppendAccessor( MeshVertexFlags.Uv2, mesh.Uv2 );
				tryAppendAccessor( MeshVertexFlags.Uv3, mesh.Uv3 );
				tryAppendAccessor( MeshVertexFlags.Color0, mesh.Color0 );
				tryAppendAccessor( MeshVertexFlags.Color1, mesh.Color1 );
				tryAppendAccessor( MeshVertexFlags.Color2, mesh.Color2 );
				tryAppendAccessor( MeshVertexFlags.Color3, mesh.Color3 );
				tryAppendAccessor( MeshVertexFlags.BoneIndices, mesh.BoneIndices );
				tryAppendAccessor( MeshVertexFlags.BoneWeights, mesh.BoneWeights );

				primitive.SetIndexAccessor(
					CreateBufferAndAccessorFromData( root, name, null, mesh.Indices ) );

				primitive.Material = useMaterial( mesh.MaterialName );
			}

			Node meshNode = rootScene.CreateNode( name );
			meshNode.Mesh = result;

			return result;
		}
	}
}
