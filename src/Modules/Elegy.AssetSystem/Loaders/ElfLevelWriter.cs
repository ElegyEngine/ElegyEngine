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
					map.RenderMeshes[i].Surfaces.Select( s => s.ToMesh() ).ToList() );
			}

			for ( int i = 0; i < map.CollisionMeshes.Count; i++ )
			{
				GltfHelpers.WriteMesh( root, $"CollisionMesh_{i}",
					map.CollisionMeshes[i].Meshlets.Select( cm => cm.ToMesh() ).ToList() );
			}

			for ( int i = 0; i < map.OccluderMeshes.Count; i++ )
			{
				GltfHelpers.WriteMesh( root, $"OccluderMesh_{i}",
					[map.OccluderMeshes[i].ToMesh()] );
			}

			Scene rootScene = root.UseScene( 0 );

			root.SaveGLB( path, new()
			{
				ImageWriting = ResourceWriteMode.Default,
				JsonIndented = true
			} );

			return true;
		}
	}
}
