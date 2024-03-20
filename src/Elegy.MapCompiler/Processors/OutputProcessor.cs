// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;

using CollisionMeshlet = Elegy.Common.Assets.ElegyMapData.CollisionMeshlet;
using RenderSurface = Elegy.Common.Assets.ElegyMapData.RenderSurface;
using RenderMesh = Elegy.Common.Assets.ElegyMapData.RenderMesh;

namespace Elegy.MapCompiler.Processors
{
	public class OutputProcessor
	{
		public ProcessingData Data { get; }
		public MapCompilerParameters Parameters { get; }

		private ElegyMapDocument mOutput = new();

		public OutputProcessor( ProcessingData data, MapCompilerParameters parameters )
		{
			Data = data;
			Parameters = parameters;
		}

		private int GetOrCreateRenderMesh( Entity entity )
		{
			Dictionary<string, RenderSurface> surfaceDict = new();

			foreach ( var brush in entity.Brushes )
			{
				foreach ( var face in brush.Faces )
				{
					if ( face.Material.HasFlag( ToolMaterialFlag.NoDraw ) )
					{
						continue;
					}

					if ( !surfaceDict.ContainsKey( face.Material.Name ) )
					{
						surfaceDict.Add( face.Material.Name, new()
						{
							Material = face.Material.Name
						} );
					}

					AddBrushFace( surfaceDict[face.Material.Name], face.Vertices );
				}
			}

			if ( surfaceDict.Count == 0 )
			{
				return -1;
			}

			RenderMesh mesh = new();
			foreach ( var surface in surfaceDict.Values )
			{
				mesh.Surfaces.Add( surface );
			}

			mOutput.RenderMeshes.Add( mesh );
			return mOutput.RenderMeshes.Count - 1;
		}

		private void AddBrushFace( RenderSurface surface, List<Vertex> vertices )
		{
			// Assumption goes that vertices are provided in the form of a polygon.
			// A triangle mesh would be added quite differently.
			for ( int i = 2; i < vertices.Count; i++ )
			{
				surface.Indices.Add( surface.VertexCount );
				surface.Indices.Add( surface.VertexCount + i - 1 );
				surface.Indices.Add( surface.VertexCount + i );
			}
			surface.VertexCount += vertices.Count;

			surface.BoundingBox = new( vertices[0].Position, Vector3.Zero );
			for ( int i = 0; i < vertices.Count; i++ )
			{
				surface.Positions.Add( vertices[i].Position );
				surface.Normals.Add( vertices[i].Normal );
				surface.Uvs.Add( vertices[i].Uv );
				surface.LightmapUvs.Add( Vector2.Zero );
				surface.Colours.Add( vertices[i].Colour );

				surface.BoundingBox = surface.BoundingBox.Expand( vertices[i].Position );
			}
		}

		int GetOrCreateCollisionMesh( Entity entity )
		{
			// A brush entity (whether worlspawn, or func_wall or whatever) can
			// have multiple faces with different materials. Different materials may
			// have different physical properties, so it's worth actually splitting them
			// up into separate collision meshes.
			Dictionary<string, List<Vector3>> collisionDict = new();

			// We have a keyvalue to force non-solidity on brush entities
			if ( entity.Pairs.ContainsKey( "elc_nonsolid" ) )
			{
				return -1;
			}

			foreach ( var brush in entity.Brushes )
			{
				// Skip non-solid brushes
				if ( brush.HasMaterialFlag( ToolMaterialFlag.NoCollision ) )
				{
					continue;
				}

				foreach ( var face in brush.Faces )
				{
					if ( !collisionDict.ContainsKey( face.Material.Name ) )
					{
						collisionDict.Add( face.Material.Name, new() );
					}

					for ( int i = 2; i < face.Vertices.Count; i++ )
					{
						var list = collisionDict[face.Material.Name];
						list.Add( face.Vertices[0].Position );
						list.Add( face.Vertices[i - 1].Position );
						list.Add( face.Vertices[i].Position );
					}
				}
			}

			// Sometimes it may happen that this is a non-solid entity
			if ( collisionDict.Count == 0 )
			{
				return -1;
			}

			List<CollisionMeshlet> meshlets = new();
			foreach ( var pair in collisionDict )
			{
				meshlets.Add( new()
				{
					Positions = pair.Value,
					MaterialName = pair.Key
				} );
			}

			mOutput.CollisionMeshes.Add( new()
			{
				Meshlets = meshlets
			} );

			return mOutput.CollisionMeshes.Count - 1;
		}

		public void GenerateOutputData()
		{
			foreach ( var entity in Data.Entities )
			{
				mOutput.Entities.Add( new()
				{
					RenderMeshId = GetOrCreateRenderMesh( entity ),
					CollisionMeshId = GetOrCreateCollisionMesh( entity ),
					OccluderMeshId = -1, // occluders are not supported yet
					Attributes = new( entity.Pairs )
				} );
			}
		}

		public void WriteToFile( string fileName )
		{
			Console.WriteLine( "[OutputProcessor] Writing to:" );
			Console.WriteLine( $"    '{fileName}'" );
			mOutput.WriteToFile( fileName );
			Console.WriteLine( "[OutputProcessor] Success" );
			return;
		}
	}
}
