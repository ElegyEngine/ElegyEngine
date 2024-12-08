// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Elegy.Common.Assets;
using Elegy.Common.Assets.ElegyMapData;
using Elegy.Common.Maths;
using Elegy.ConsoleSystem;
using Elegy.ECS;
using Elegy.RenderSystem.Objects;
using Game.Presentation;
using Game.Server;
using Mesh = Elegy.Common.Assets.MeshData.Mesh;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Worldspawn
	{
		private static TaggedLogger mLogger = new( "Worldspawn" );
		[Property] public string Name { get; set; }
		public MeshEntity? MeshEntity { get; set; } // not null on clientside
		public Model Model { get; set; }

		[Event]
		public void OnMapLoad( Entity.OnMapLoadEvent data )
		{
			mLogger.Log( "OnMapLoad" );
			Model = CreateModelFromMap( data.MapDocument );
		}

		[Event]
		public void OnClientSpawn( Entity.ClientSpawnEvent data )
		{
			mLogger.Log( "OnClientSpawn" );
			MeshEntity = StaticModel.CreateMeshEntity( false, Model, Vector3.Zero, Vector3.Zero );
		}

		[Event]
		public void OnRender( Renderer.RenderEvent data )
		{
			data.RenderContext.QueueMeshEntity( MeshEntity );
		}

		public static Model CreateModelFromMap( ElegyMapDocument map )
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

			result.Name = "worldspawn";
			foreach ( var renderMesh in map.RenderMeshes )
			{
				foreach ( var renderSurface in renderMesh.Surfaces )
				{
					result.Meshes.Add( RenderSurfaceToMesh( renderSurface ) );
				}
			}

			return result;
		}
	}
}
