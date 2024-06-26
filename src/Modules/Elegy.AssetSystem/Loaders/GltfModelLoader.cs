﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.Common.Assets;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.ConsoleSystem;

using EngineMesh = Elegy.Common.Assets.MeshData.Mesh;

using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System.Numerics;

namespace Elegy.AssetSystem.Loaders
{
	/// <summary>
	/// Built-in GLTF loader.
	/// </summary>
	public class GltfModelLoader : BaseAssetLoader, IModelLoader
	{
		private TaggedLogger mLogger = new( "GltfLoader" );

		/// <inheritdoc/>
		public override string Name => "GltfModelLoader";

		/// <inheritdoc/>
		public override bool CanLoad( string extension )
			=> extension switch
			{
				".gltf" => true,
				".glb" => true,
				_ => false
			};

		/// <inheritdoc/>
		public Model? LoadModel( string path )
		{
			ModelRoot gltfScene;
			try
			{
				gltfScene = SceneBuilder.LoadDefaultScene( path, new()
				{
					Validation = SharpGLTF.Validation.ValidationMode.Skip
				} )
				.ToGltf2( new()
				{
					MergeBuffers = false,
					UseStridedBuffers = true
				} );
			}
			catch ( Exception ex )
			{
				mLogger.Error( $"Couldn't load model {path}, message:\n{ex.Data}" );
				return null;
			}

			List<EngineMesh> engineMeshes = new();

			foreach ( var mesh in gltfScene.LogicalMeshes )
			{
				mLogger.Log( $"Mesh '{mesh.Name}'" );
				foreach ( var primitive in mesh.Primitives )
				{
					EngineMesh engineMesh = new();

					mLogger.Log( $"  * Primitive {primitive.GetHashCode()}" );
					mLogger.Log( $"    * Material {primitive.Material.Name}" );

					foreach ( var vertexAccessor in primitive.VertexAccessors )
					{
						mLogger.Log( $"    * Vertex input {vertexAccessor.Key}" );

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

					engineMeshes.Add( engineMesh );
				}
			}

			return new Model()
			{
				Name = gltfScene.LogicalScenes[0].Name,
				FullPath = path,
				Meshes = engineMeshes
			};
		}

		private Vector3[] LoadPositions( Accessor value )
			=> value.AsVector3Array().ToArray();

		private Vector3[] LoadNormals( Accessor value )
			=> value.AsVector3Array().ToArray();

		private Vector4[] LoadTangents( Accessor value )
			=> value.AsVector4Array().ToArray();

		private Vector2[] LoadUvs( Accessor value )
			=> value.AsVector2Array().ToArray();

		private Vector4B[] LoadColours( Accessor value )
			=> EngineMesh.Transform( value.AsColorArray( 1.0f ),
				transform: ( vertex ) => new Vector4B( (byte)vertex.X, (byte)vertex.Y, (byte)vertex.Z, byte.MaxValue ) );

		private Vector4B[] LoadJoints( Accessor value )
		{
			mLogger.Log( $"      * Joints: {value.Encoding}, {value.Dimensions}, {value.Format}" );

			Vector4B[] list = new Vector4B[value.Count];
			ByteBuffer byteBuffer = new( value.SourceBufferView.Content.Array );
			for ( int i = 0; i < value.Count; i++ )
			{
				list[i] = byteBuffer.Read<Vector4B>();
			}

			return list;
		}

		private Vector4[] LoadWeights( Accessor value )
			=> value.AsVector4Array().ToArray();

		private uint[] LoadIndices( Accessor value )
			=> value.AsIndicesArray().ToArray();
	}
}
