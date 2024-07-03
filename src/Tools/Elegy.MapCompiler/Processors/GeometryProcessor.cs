// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;

namespace Elegy.MapCompiler.Processors
{
	public class GeometryProcessor
	{
		public ProcessingData Data { get; }
		public MapCompilerParameters Parameters { get; }

		public GeometryProcessor( ProcessingData data, MapCompilerParameters parameters )
		{
			Data = data;
			Parameters = parameters;
		}

		private void ValidateBrush( BrushMapBrush brush )
		{
			foreach ( var face in brush.Faces )
			{
				GeoValidation.Vec3( face.PlaneDefinition[0], "Plane definition bad, something went wrong with map export" );
				GeoValidation.Vec3( face.Polygon.Origin, "Face centre bad, couldn't generate polygons from this" );
			}
		}

		public void GenerateGeometryFromMap( BrushMapDocument map )
		{
			map.MergeInto( "worldspawn", "func_group" );

			Data.Entities.Capacity = map.MapEntities.Count;
			foreach ( var entity in map.MapEntities )
			{
				foreach ( var brush in entity.Brushes )
				{
					brush.IntersectPlanes();
					ValidateBrush( brush );
				}

				Data.Entities.Add( new( entity ) );
			}
		}

		public void RemoveFacesWithFlags( ToolMaterialFlag flags )
		{
			foreach ( var entity in Data.Entities )
			{
				foreach ( var brush in entity.Brushes )
				{
					foreach ( var face in brush.Faces )
					{
						if ( face.Material.Data.ToolFlags.HasFlag( flags ) )
						{
							brush.Faces.Remove( face );
						}
					}

					if ( brush.Faces.Count == 0 )
					{
						entity.Brushes.Remove( brush );
					}
				}
			}
		}

		public void UpdateBoundaries()
		{
			Data.MapBoundaries = new Box3( Vector3.Zero, Vector3.One * 0.1f );

			foreach ( var entity in Data.Entities )
			{
				GeoValidation.Vec3( entity.Centre, "Entity centre invalid" );

				for ( int i = 0; i < 8; i++ )
				{
					Vector3 point = entity.Centre + entity.BoundingBox.GetEndpoint( i );
					Data.MapBoundaries = Data.MapBoundaries.Expand( point );
				}
			}
		}

		public void FixBrushOrigins()
		{
			foreach ( var entity in Data.Entities )
			{
				// Skip point entities
				if ( entity.Brushes.Count == 0 || entity.Pairs.ContainsKey( "origin" ) )
				{
					continue;
				}

				// Worldspawn's origin must be 0,0,0
				if ( entity.ClassName == "worldspawn" )
				{
					continue;
				}

				// Obtain brush entity origin from origin brushes
				Vector3 brushOrigin = Vector3.Zero;
				float brushOriginCount = 0.0f;
				foreach ( var brush in entity.Brushes )
				{
					if ( brush.HasMaterialFlag( ToolMaterialFlag.Origin ) )
					{
						brushOrigin += brush.BoundingBox.GetCenter();
						brushOriginCount += 1.0f;
					}
				}

				// If there's no origin brushes associated with this entity,
				// form one implicitly from the bounding box centre
				if ( brushOriginCount == 0.0f )
				{
					foreach ( var brush in entity.Brushes )
					{
						brushOrigin += brush.BoundingBox.GetCenter();
						brushOriginCount += 1.0f;
					}
				}
				brushOrigin /= brushOriginCount;

				// Shift the entity's centre + all other geometry so that the world position of
				// the vertices stays the same. Just different relative to the origin
				Vector3 originDelta = entity.Centre - brushOrigin;
				foreach ( var brush in entity.Brushes )
				{
					brush.Move( -originDelta );
				}
				entity.Centre = brushOrigin;
				entity.Pairs.SetVector3( "origin", entity.Centre );

				// Finally regenerate the bounding boxes,
				// as they're out of date now
				entity.RegenerateBounds();
			}
		}
	}
}
