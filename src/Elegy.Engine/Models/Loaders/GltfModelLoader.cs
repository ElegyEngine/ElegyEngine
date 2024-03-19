// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;

namespace Elegy.ModelLoaders
{
	/// <summary>
	/// Built-in GLTF loader.
	/// </summary>
	public class GltfModelLoader : IModelLoader
	{
		private TaggedLogger mLogger = new();

		/// <inheritdoc/>
		public bool CanLoad( string path )
		{
			if ( !path.EndsWith( ".gltf" ) && !path.EndsWith( ".glb" ) )
			{
				return false;
			}

			return true;
		}

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

						switch ( vertexAccessor.Key )
						{	// TODO: fill in the data
							case "POSITION": break;
							case "NORMAL": break;
							case "UV0": break;
							case "UV1": break;
							case "COLOR0": break;
							case "COLOR1": break;
							case "TANGENT": break;
							case "BITANGENT": break;
						}
					}
				}
			}

			return new Model()
			{
				Name = path,
				Meshes = new()
			};
		}
	}
}
