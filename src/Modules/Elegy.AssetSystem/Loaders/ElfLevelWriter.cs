// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.Common.Assets;
using Elegy.Common.Assets.ElegyMapData;
using Elegy.Common.Assets.GltfExtensions;
using Elegy.Common.Extensions;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;

using EngineMesh = Elegy.Common.Assets.MeshData.Mesh;

using SharpGLTF.Schema2;

namespace Elegy.AssetSystem.Loaders
{
	public class ElfLevelWriter : BaseAssetIo, ILevelWriter
	{
		public override string Name => "ElfWriter";

		public override bool Supports( string extension )
			=> extension == ".elf";

		public bool WriteLevel( string path, ElegyMapDocument map )
		{
			var root = ModelRoot.CreateModel();

			ElegyLevelExtension extension = new()
			{
				WorldMeshIds = map.WorldMeshIds,
				Entities = map.Entities
			};

			root.SetExtension( extension );

			for ( int i = 0; i < map.RenderMeshes.Count; i++ )
			{
				GltfHelpers.WriteMesh( root, $"RenderMesh_{i}",
					map.RenderMeshes[i].Surfaces.Select( SurfaceToEngineMesh ).ToList() );
			}

			for ( int i = 0; i < map.CollisionMeshes.Count; i++ )
			{
				GltfHelpers.WriteMesh( root, $"CollisionMesh_{i}",
					map.CollisionMeshes[i].Meshlets.Select( CollisionMeshletToEngineMesh ).ToList() );
			}

			for ( int i = 0; i < map.OccluderMeshes.Count; i++ )
			{
				GltfHelpers.WriteMesh( root, $"OccluderMesh_{i}",
					[OccluderToEngineMesh( map.OccluderMeshes[i] )] );
			}

			root.SaveGLB( path );
			return true;
		}

		private EngineMesh SurfaceToEngineMesh( RenderSurface surface )
			=> new()
			{
				Positions = surface.Positions.ToArray(),
				Normals = surface.Normals.ToArray(),
				Uv0 = surface.Uvs.ToArray(),
				Uv1 = surface.LightmapUvs.ToArray(),
				Color0 = surface.Colours.Select( v => (Vector4B)v ).ToArray(),
				Indices = surface.Indices.Select( i => (uint)i ).ToArray(),
				MaterialName = surface.Material
			};

		private EngineMesh CollisionMeshletToEngineMesh( CollisionMeshlet meshlet )
		{
			var uniquePositionIndices = meshlet.Positions.ToVectorIndexDictionary();

			EngineMesh result = new()
			{
				MaterialName = meshlet.MaterialName,
				Positions = uniquePositionIndices.Keys.ToArray(),
				Indices = meshlet.Positions.Select( v =>
				{
					return (uint)uniquePositionIndices[v];
				} ).ToArray()
			};

			return result;
		}

		private EngineMesh OccluderToEngineMesh( OccluderMesh occluder )
			=> new()
			{
				Positions = occluder.Positions.ToArray(),
				Indices = occluder.Indices.Select( i => (uint)i ).ToArray(),
				MaterialName = "null"
			};
	}
}
