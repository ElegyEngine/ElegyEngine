// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;

using CollisionMeshlet = Elegy.Common.Assets.ElegyMapData.CollisionMeshlet;
using RenderSurface = Elegy.Common.Assets.ElegyMapData.RenderSurface;
using RenderMesh = Elegy.Common.Assets.ElegyMapData.RenderMesh;
using Elegy.ConsoleSystem;

namespace Elegy.MapCompiler.Processors
{
	public class OutputProcessor
	{
		public ProcessingData Data { get; }
		public MapCompilerParameters Parameters { get; }

		private TaggedLogger mLogger = new( "OutputProc" );
		private ElegyMapDocument mOutput = new();

		public OutputProcessor( ProcessingData data, MapCompilerParameters parameters )
		{
			Data = data;
			Parameters = parameters;
		}

		private int GetOrCreateRenderMesh( Entity entity )
		{
			Dictionary<string, RenderSurface> surfaceDict = new();

			foreach ( var face in entity.Faces )
			{
				if ( face.Material.Data.ToolFlags.HasFlag( ToolMaterialFlag.NoDraw ) )
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
				GeoValidation.Vec3( vertices[i].Position, $"Vertex {i} invalid" );

				surface.Positions.Add( vertices[i].Position );
				surface.Normals.Add( vertices[i].Normal );
				surface.Uvs.Add( vertices[i].Uv );
				surface.LightmapUvs.Add( Vector2.Zero );
				surface.Colours.Add( vertices[i].Colour );

				surface.BoundingBox = surface.BoundingBox.Expand( vertices[i].Position );
			}
		}

		private int GetOrCreateCollisionMesh( Entity entity )
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

			foreach ( var face in entity.Faces )
			{
				// Skip non-solid brushes
				if ( face.HasMaterialFlag( ToolMaterialFlag.NoCollision ) )
				{
					continue;
				}

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

		private static bool IsVertexUnique( RenderSurface optimisedSurface, RenderSurface originalSurface, int vertexIndex )
		{
			const float radiusTolerance = 1.0f / 256.0f;
			const float radiusTolerancePosition = 1.0f / 64.0f;

			for ( int i = 0; i < optimisedSurface.VertexCount; i++ )
			{
				bool samePositions = optimisedSurface.Positions[i].IsEqualApprox( originalSurface.Positions[vertexIndex], radiusTolerancePosition );
				bool sameNormals = optimisedSurface.Normals[i].IsEqualApprox( originalSurface.Normals[vertexIndex], radiusTolerance );
				bool sameUvs = optimisedSurface.Uvs[i].IsEqualApprox( originalSurface.Uvs[vertexIndex], radiusTolerance );
				bool sameLightmapUvs = optimisedSurface.LightmapUvs[i].IsEqualApprox( originalSurface.LightmapUvs[vertexIndex], radiusTolerance );
				bool sameColours = optimisedSurface.Colours[i] == originalSurface.Colours[vertexIndex];

				// Usually this condition will happen on duplicated vertices, and that's what we're optimising for.
				// It may also help seal some micro gaps, and produce degenerate triangles, i.e. triangles which
				// point to the same 3 vertices now that they've been reduced. That is dealt with elsewhere
				if ( samePositions && sameNormals && sameUvs && sameLightmapUvs && sameColours )
				{
					return false;
				}
			}

			return true;
		}

		public void OptimiseRenderSurfaces()
		{
			foreach ( var entity in mOutput.Entities )
			{
				if ( entity.RenderMeshId < 0 )
				{
					continue;
				}

				var mesh = mOutput.RenderMeshes[entity.RenderMeshId];
				List<RenderSurface> optimisedRenderSurfaces = new( mesh.Surfaces.Count );

				foreach ( var surface in mesh.Surfaces )
				{
					RenderSurface optimisedSurface = new();
					List<int> vertexIndexRemap = new();

					for ( int i = 0; i < surface.VertexCount; i++ )
					{
						vertexIndexRemap.Add( optimisedSurface.VertexCount );

						if ( IsVertexUnique( optimisedSurface, surface, i ) )
						{
							optimisedSurface.AddVertex(
								surface.Positions[i],
								surface.Normals[i],
								surface.Uvs[i],
								surface.LightmapUvs[i],
								surface.Colours[i] );
						}
					}

					for ( int i = 0; i < surface.Indices.Count; i += 3 )
					{
						int a = vertexIndexRemap[i];
						int b = vertexIndexRemap[i + 1];
						int c = vertexIndexRemap[i + 2];

						if ( a == b || a == c || b == c )
						{
							// Degenerate micro triangle, got reduced by optimising
							mLogger.Warning( $"Degenerate micro triangle: ({optimisedSurface.Positions[a]})" );
							continue;
						}

						optimisedSurface.AddTriangle( vertexIndexRemap[i], vertexIndexRemap[i + 1], vertexIndexRemap[i + 2] );
					}

					optimisedRenderSurfaces.Add( optimisedSurface );
				}
			}
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
			mOutput.WriteToFile( fileName );
		}
	}
}
