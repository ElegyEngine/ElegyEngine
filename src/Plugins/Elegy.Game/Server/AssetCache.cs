// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Assets.ElegyMapData;
using Elegy.Common.Assets.MeshData;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.LogSystem;
using Game.Shared;

namespace Game.Server
{
	/// <summary>
	/// Handles asset precaching on the server so it can be sent to clients.
	/// </summary>
	public static class AssetCache
	{
		private static TaggedLogger mLogger = new( "AssetCache" );

		public static AssetRegistry Registry { get; } = new();
		public static ElegyMapDocument MapDocument { get; private set; }

		public static Dictionary<string, int> ModelRefs { get; } = new();
		public static Dictionary<string, int> MaterialRefs { get; } = new();
		public static Dictionary<string, int> SoundRefs { get; } = new();
		public static Dictionary<string, int> OtherFileRefs { get; } = new();

		private static Model CreateBrushModel( int meshId )
		{
			Mesh RenderSurfaceToMesh( RenderSurface surface )
				=> new()
				{
					Indices = surface.Indices.Select( i => (uint)i ).ToArray(),
					Positions = surface.Positions.ToArray(),
					Normals = surface.Normals.ToArray(),
					Uv0 = surface.Uvs.ToArray(),
					Uv1 = surface.LightmapUvs.ToArray(),
					Color0 = surface.Colours.Select( v => (Vector4B)v ).ToArray(),
					MaterialName = surface.Material
				};

			Model result = new();

			result.Name = $"*{meshId}";
			foreach ( var renderSurface in MapDocument.RenderMeshes[meshId].Surfaces )
			{
				result.Meshes.Add( RenderSurfaceToMesh( renderSurface ) );
			}

			return result;
		}

		private static Model? LoadModelInternal( string name )
		{
			if ( name.StartsWith( '*' ) )
			{
				int meshId = Parse.Int( name[1..] );
				return CreateBrushModel( meshId );
			}

			return Assets.LoadModel( name );
		}

		public static void InitLevel( ElegyMapDocument mapDocument )
		{
			MapDocument = mapDocument;
		}

		public static Model? LoadModel( string name )
		{
			if ( Registry.Models.TryGetValue( name, out Model? model ) )
			{
				if ( model is not null )
				{
					return model;
				}
			}

			model = LoadModelInternal( name );
			Registry.Models[name] = model;
			return model;
		}

		public static void LinkModel( string name )
		{
			if ( ModelRefs.TryGetValue( name, out int count ) )
			{
				ModelRefs[name] = count + 1;
			}
		}

		public static void UnlinkModel( string name )
		{
			if ( ModelRefs.TryGetValue( name, out int count ) )
			{
				ModelRefs[name] = count - 1;
			}
		}
	}
}
