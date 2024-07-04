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
	public class GltfModelLoader : BaseAssetIo, IModelLoader
	{
		private TaggedLogger mLogger = new( "GltfLoader" );

		/// <inheritdoc/>
		public override string Name => "GltfModelLoader";

		/// <inheritdoc/>
		public override bool Supports( string extension )
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
					mLogger.Log( $"  * Primitive {primitive.GetHashCode()}" );
					mLogger.Log( $"    * Material {primitive.Material.Name}" );
					foreach ( var vertexAccessor in primitive.VertexAccessors )
					{
						mLogger.Log( $"    * Vertex input {vertexAccessor.Key}" );
					}

					engineMeshes.Add( GltfHelpers.LoadMesh( primitive ) );
				}
			}

			return new Model()
			{
				Name = gltfScene.LogicalScenes[0].Name,
				FullPath = path,
				Meshes = engineMeshes
			};
		}
	}
}
