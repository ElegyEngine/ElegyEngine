// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Geometry;

namespace Elegy.Assets.BrushMapData
{
	public class Brush
	{
		public Vector3 Centre = Vector3.Zero;
		public Aabb BoundingBox = new();
		public List<Face> Faces = new();

		public void IntersectPlanes()
		{
			// Radius is longestAxis * 2.5 because there is a chance the
			// reported face centre is actually a corner or near a corner.
			CreateBrushPolygons( Faces, BoundingBox.GetLongestAxisSize() * 2.5f );
		}

		private static void IntersectPolygonWithOthers( ref Polygon3D polygon, List<Face> faces, int skipIndex )
		{
			for ( int i = 0; i < faces.Count; i++ )
			{
				Plane intersector = faces[i].Plane;
				if ( i == skipIndex )
				{
					continue;
				}

				var splitResult = polygon.Split( intersector );
				if ( splitResult.DidIntersect )
				{
					// Modify the polygon we started off from
					polygon = splitResult.Back ?? polygon;
				}
			}
		}

		private static void CreateBrushPolygons( List<Face> faces, float radius )
		{
			for ( int i = 0; i < faces.Count; i++ )
			{
				Plane plane = faces[i].Plane;

				// Create a polygon in the centre of the world
				Polygon3D poly = new Polygon3D( plane, radius );

				// Then align its centre to the centre of this face... if we got any
				// Otherwise precision issues will occur
				Vector3 shift = faces[i].Centre - poly.Origin;
				poly.Shift( shift );

				// Intersect current face with all other faces
				IntersectPolygonWithOthers( ref poly, faces, i );

				// Axis:    Quake:   Godot:
				// Forward  +X       -Z
				// Right    -Y       +X
				// Up       +Z       +Y
				for ( int p = 0; p < poly.Points.Count; p++ )
				{
					poly.Points[p] = poly.Points[p]
						// Snap to a grid of 0.25 to avoid some micro gaps
						// TODO: move this into a processing pass
						.Snapped( Vector3.One * 0.25f );
				}

				// Finally add the subdivided polygon
				faces[i].Polygon = poly;
			}
		}
	}
}
