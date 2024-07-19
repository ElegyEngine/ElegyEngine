// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.Common.Assets;
using Elegy.Common.Assets.ElegyMapData;
using Elegy.Common.Assets.GltfExtensions;
using Elegy.Common.Maths;
using Elegy.ConsoleSystem;

using EngineMesh = Elegy.Common.Assets.MeshData.Mesh;

using SharpGLTF.Schema2;
using System.Numerics;
using Elegy.Common.Utilities;

namespace Elegy.AssetSystem.Loaders
{
	/// <summary>
	/// Built-in ELF loader.
	/// </summary>
	public class ElfLevelLoader : BaseAssetIo, ILevelLoader
	{
		private TaggedLogger mLogger = new( "ElfLoader" );

		/// <inheritdoc/>
		public override string Name => "ElfLevelLoader";

		/// <inheritdoc/>
		public override bool Supports( string extension )
			=> extension == ".elf";

		public override bool Init()
		{
			ExtensionsFactory.RegisterExtension<ModelRoot, ElegyLevelExtension>( "ELEGY_level_data", root => new ElegyLevelExtension( root ) );

			return true;
		}

		/// <inheritdoc/>
		public ElegyMapDocument? LoadLevel( string path )
		{
			ModelRoot gltfScene;
			try
			{
				gltfScene = ModelRoot.Load( path, new()
				{
					Validation = SharpGLTF.Validation.ValidationMode.Skip
				} );
			}
			catch ( Exception ex )
			{
				mLogger.Error( $"Couldn't load level {path}, message:\n{ex.Data}" );
				return null;
			}

			var levelData = gltfScene.GetExtension<ElegyLevelExtension>();
			var gltfMeshes = gltfScene.LogicalMeshes;

			return new()
			{
				WorldMeshIds = levelData.WorldMeshIds,
				Entities = levelData.Entities,

				RenderMeshes = LoadMeshes( gltfMeshes, "RenderMesh" )
					.Select( mesh => new RenderMesh()
					{
						Surfaces = mesh.Select( ConvertMeshToRenderSurface ).ToList()
					} )
					.ToList(),

				CollisionMeshes = LoadMeshes( gltfMeshes, "CollisionMesh" )
					.Select( mesh => new CollisionMesh()
					{
						Meshlets = mesh.Select( ConvertMeshToCollisionMeshlet ).ToList()
					} )
					.ToList(),

				OccluderMeshes = LoadMeshes( gltfMeshes, "OccluderMesh" )
					.Select( ConvertMeshesToOccluderMesh )
					.ToList()
			};
		}

		private static RenderSurface ConvertMeshToRenderSurface( EngineMesh mesh )
			=> new()
			{
				BoundingBox = new Box3(),
				Positions = mesh.Positions.ToList(),
				Normals = mesh.Normals.ToList(),
				Uvs = mesh.Uv0.ToList(),
				LightmapUvs = mesh.Uv1.ToList(),
				// TODO: better conversion utilities
				Colours = EngineMesh.Transform( mesh.Color0, v => new Vector4( v.X, v.Y, v.Z, v.W ) / 255.0f ).ToList(),
				Indices = mesh.Indices.Select( index => (int)index ).ToList(),
				VertexCount = mesh.Positions.Length,
				Material = mesh.MaterialName
			};

		private static CollisionMeshlet ConvertMeshToCollisionMeshlet( EngineMesh mesh )
		{
			CollisionMeshlet result = new()
			{
				MaterialName = mesh.MaterialName
			};

			result.Positions.EnsureCapacity( mesh.Indices.Length );

			for ( int i = 0; i < mesh.Indices.Length; i++ )
			{
				result.Positions.Add( mesh.Positions[mesh.Indices[i]] );
			}

			return result;
		}

		private static OccluderMesh ConvertMeshesToOccluderMesh( List<EngineMesh> meshes )
		{
			OccluderMesh result = new();

			result.Positions.EnsureCapacity( meshes.Sum( mesh => mesh.Positions.Length ) );
			result.Indices.EnsureCapacity( meshes.Sum( mesh => mesh.Indices.Length ) );

			int indexOffset = 0;
			foreach ( var mesh in meshes )
			{
				result.Positions.AddRange( mesh.Positions );
				result.Indices.AddRange( mesh.Indices.Select( i => (int)i + indexOffset ) );
				indexOffset += mesh.Indices.Length;
			}

			return result;
		}

		private List<List<EngineMesh>> LoadMeshes( IReadOnlyList<Mesh> gltfMeshes, string filter )
		{
			return gltfMeshes.Where( mesh => mesh.Name.StartsWith( filter ) )
				.Select( mesh => mesh.Primitives
					.Select( p => GltfHelpers.LoadMesh( p, yIntoZ: true ) )
					.ToList() )
				.ToList();
		}
	}
}
