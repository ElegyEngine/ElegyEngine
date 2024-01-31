// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy;
using Elegy.Assets;
using Elegy.Assets.ElegyMapData;
using Elegy.Geometry;

namespace TestGame.Assets
{
	public static class MapGeometry
	{
		/*
		private static void AppendMapSurfaceToMesh( ArrayMesh mesh, Material material, RenderSurface surface )
		{
			SurfaceTool builder = new();
			builder.Begin( Mesh.PrimitiveType.Triangles );

			for ( int vertexId = 0; vertexId < surface.Positions.Count; vertexId++ )
			{
				builder.SetUV( surface.Uvs[vertexId] );
				builder.SetNormal( surface.Normals[vertexId].ToGodot( scale: 1.0f ) );
				builder.AddVertex( surface.Positions[vertexId].ToGodot() );
			}

			surface.Indices.ForEach( index => builder.AddIndex( index ) );
			builder.GenerateTangents();
			builder.SetMaterial( material );
			builder.Commit( mesh );
		}
		*/

		/*
		public static Node3D CreateBrushModelNode( ElegyMapDocument map, int brushEntityId )
		{ 
			var brushEntity = map.Entities[brushEntityId];
			var brushMesh = map.RenderMeshes[brushEntity.RenderMeshId];

			Vector3 brushOrigin = brushEntity.Attributes.GetVector3( "origin" ).ToGodot();

			Dictionary<string, Material> materialDictionary = new();
			Dictionary<string, RenderSurface> renderSurfaces = new();
			brushMesh.Surfaces.ForEach( surface =>
			{
				renderSurfaces[surface.Material] = surface;
			} );

			foreach ( var materialString in renderSurfaces.Keys )
			{
				materialDictionary[materialString] = Materials.LoadMaterial( materialString );
			}

			ArrayMesh mesh = new ArrayMesh();
			for ( int renderSurfaceId = 0; renderSurfaceId < renderSurfaces.Count; renderSurfaceId++ )
			{
				RenderSurface surface = renderSurfaces.Values.ElementAt( renderSurfaceId );

				AppendMapSurfaceToMesh( mesh, materialDictionary[surface.Material], surface );
			}

			Node3D parentNode = Nodes.CreateNode<Node3D>();
			parentNode.Name = brushEntity.Attributes["classname"];

			MeshInstance3D meshInstance = parentNode.CreateChild<MeshInstance3D>();
			meshInstance.Mesh = mesh;
			meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.DoubleSided;

			// TODO: load collision mesh separately instead of using the visual mesh
			StaticBody3D staticBody = parentNode.CreateChild<StaticBody3D>();
			CollisionShape3D collisionShape = staticBody.CreateChild<CollisionShape3D>();
			collisionShape.Shape = Nodes.CreateCollisionShape( mesh );

			return parentNode;
		}
		*/
	}
}
