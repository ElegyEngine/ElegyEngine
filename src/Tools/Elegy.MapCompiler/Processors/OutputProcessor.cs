// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;
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

		private TaggedLogger mLogger = new( "Output" );
		private ElegyMapDocument mOutput = new();

		public OutputProcessor( ProcessingData data, MapCompilerParameters parameters )
		{
			mLogger.Log( "Init" );

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
				surface.Indices.Add( surface.VertexCount + i );
				surface.Indices.Add( surface.VertexCount + i - 1 );
				surface.Indices.Add( surface.VertexCount );
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
					list.Add( face.Vertices[i].Position );
					list.Add( face.Vertices[i - 1].Position );
					list.Add( face.Vertices[0].Position );
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

		private static int TryAddUniqueVertex( RenderSurface optimisedSurface, RenderSurface originalSurface, int vertexIndex, float scale )
		{
			float radiusTolerance = (1.0f / 128.0f); // This is effectively like compressing the normals into bytes
			float radiusTolerancePosition = (1.0f / 5.0f) * scale; // Fifth of a TrenchBroom unit, just about half a centimetre

			if ( optimisedSurface.VertexCount == 0 )
			{
				return -1;
			}

			for ( int i = 0; i < optimisedSurface.VertexCount; i++ )
			{
				Vector3 positionA = optimisedSurface.Positions[i];
				Vector3 positionB = originalSurface.Positions[vertexIndex];

				bool samePositions = positionA.IsEqualApprox( positionB, radiusTolerancePosition );
				bool sameNormals = optimisedSurface.Normals[i].IsEqualApprox( originalSurface.Normals[vertexIndex], radiusTolerance );
				bool sameUvs = optimisedSurface.Uvs[i].IsEqualApprox( originalSurface.Uvs[vertexIndex], radiusTolerance );
				bool sameLightmapUvs = optimisedSurface.LightmapUvs[i].IsEqualApprox( originalSurface.LightmapUvs[vertexIndex], radiusTolerance );
				bool sameColours = optimisedSurface.Colours[i] == originalSurface.Colours[vertexIndex];

				// Usually this condition will happen on duplicated vertices, and that's what we're optimising for.
				// It may also help seal some micro gaps, and produce degenerate triangles, i.e. triangles which
				// point to the same 3 vertices now that they've been reduced. That is dealt with elsewhere
				if ( samePositions && sameNormals && sameUvs && sameLightmapUvs && sameColours )
				{
					return i;
				}
			}

			return -1;
		}

		public void OptimiseRenderSurfaces()
		{
			mLogger.Log( "OptimiseRenderSurfaces" );

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

					for ( int i = 0; i < surface.Indices.Count; i++ )
					{
						int vertexIndex = surface.Indices[i];

						int newVertexIndex = TryAddUniqueVertex( optimisedSurface, surface, vertexIndex, Parameters.GlobalScale );
						if ( newVertexIndex < 0 )
						{
							vertexIndexRemap.Add( optimisedSurface.VertexCount );

							optimisedSurface.AddVertex(
								surface.Positions[vertexIndex],
								surface.Normals[vertexIndex],
								surface.Uvs[vertexIndex],
								surface.LightmapUvs[vertexIndex],
								surface.Colours[vertexIndex] );
						}
						else
						{
							vertexIndexRemap.Add( newVertexIndex );
						}
					}

					for ( int i = 2; i < surface.Indices.Count; i += 3 )
					{
						int a = vertexIndexRemap[i - 2];
						int b = vertexIndexRemap[i - 1];
						int c = vertexIndexRemap[i];

						if ( a == b || a == c || b == c )
						{
							// Degenerate micro triangle, got reduced by optimising
							mLogger.Warning( $"Degenerate micro triangle: ({optimisedSurface.Positions[a] * Parameters.UnitsPerMetre})" );
							continue;
						}

						optimisedSurface.AddTriangle( a, b, c );
					}

					optimisedSurface.Material = surface.Material;

					optimisedRenderSurfaces.Add( optimisedSurface );
				}

				mesh.Surfaces.Clear();
				mesh.Surfaces = optimisedRenderSurfaces;
			}
		}

		public void GenerateOutputData()
		{
			mLogger.Log( "GenerateOutputData" );

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

			mLogger.Log( $"  * Entities:      {mOutput.Entities.Count}" );
			mLogger.Log( $"  * Render meshes: {mOutput.RenderMeshes.Count}" );
			mLogger.Log( $"  * Coll. meshes:  {mOutput.CollisionMeshes.Count}" );
			mLogger.Log( $"  * Occl. meshes:  {mOutput.OccluderMeshes.Count}" );
		}

		public ElegyMapDocument GetOutputData()
		{
			return mOutput;
		}
	}
}
