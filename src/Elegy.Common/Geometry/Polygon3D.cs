// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Maths;

namespace Elegy.Geometry
{
	public struct Polygon3DSplitResult
	{
		public Polygon3DSplitResult()
		{

		}

		public static bool operator true( Polygon3DSplitResult psr )
		{
			return psr.DidIntersect;
		}

		public static bool operator false( Polygon3DSplitResult psr )
		{
			return !psr.DidIntersect;
		}

		public bool DidIntersect = false;
		public Polygon3D? Front = null;
		public Polygon3D? Back = null;
		public Polygon3D? CoplanarFront = null;
		public Polygon3D? CoplanarBack = null;
	}

	// 3D polygon with at least 3 vertices
	public struct Polygon3D
	{
		#region Constructors
		public Polygon3D( List<Vector3D> points )
		{
			Points = points;
		}

		public Polygon3D( IEnumerable<Vector3D> points )
		{
			Points = points.ToList();
		}

		public Polygon3D( Vector3D a, Vector3D b, Vector3D c )
		{
			Points = new()
			{
				a, b, c
			};
		}

		public Polygon3D( PlaneD plane, double radius )
		{
			Vector3D direction = plane.GetClosestAxis();
			Vector3D bidirection = direction == Vector3D.UnitZ ? -Vector3D.UnitX : -Vector3D.UnitZ;

			Vector3D up = bidirection.Cross( plane.Normal ).Normalized();
			Vector3D right = plane.Normal.Cross( up ).Normalized();

			Vector3D pointOnPlane = plane.GetCenter();

			Vector3D[] planePoints = new Vector3D[4]
			{
				pointOnPlane + right + up, // Top right
				pointOnPlane - right + up, // Top left
				pointOnPlane - right - up, // Bottom left
				pointOnPlane + right - up // Bottom right
			};

			Vector3D centre = planePoints.Average();

			Points = planePoints.Select( v => (v - centre).Normalized() * radius + centre ).ToList();
		}
		#endregion

		#region Properties
		public PlaneD Plane => PlaneD.CreateFromVertices( Points[0], Points[1], Points[2] );
		public Vector3D Origin
		{
			get
			{
				Vector3D sum = Vector3D.Zero;
				Points.ForEach( v => sum += v );
				return sum / Points.Count;
			}
		}
		#endregion

		public bool IsValid( bool requirePlanar = false )
		{
			if ( Points.Count < 3 )
			{
				return false;
			}

			if ( requirePlanar )
			{
				PlaneD plane = Plane;
				return Points.TrueForAll( v => plane.HasPoint( v ) );
			}

			return true;
		}

		public void Shift( Vector3D shift )
		{
			for ( int i = 0; i < Points.Count; i++ )
			{
				Points[i] = Points[i] + shift;
			}
		}

		public bool Split( PlaneD plane, out Polygon3D? back, out Polygon3D? front )
		{
			return Split( plane, out back, out front, out _, out _ );
		}

		public bool Split( PlaneD plane, out Polygon3D? back, out Polygon3D? front, out Polygon3D? coplanarBack, out Polygon3D? coplanarFront )
		{
			var result = Split( plane );

			back = result.Back;
			front = result.Front;
			coplanarBack = result.CoplanarBack;
			coplanarFront = result.CoplanarFront;

			return result.DidIntersect;
		}

		public Polygon3DSplitResult Split( PlaneD plane )
		{
			Polygon3DSplitResult result = new();

			// Points at negative distances will become part of the "back" polygon,
			// and points at positive distances will become part of the "front" polygon
			var distances = Points.Select( p => plane.DistanceTo( p ) ).ToList();

			int numFrontPoints = 0, numBackPoints = 0;
			for ( int i = 0; i < distances.Count; i++ )
			{
				if ( distances[i] < 0.0f )
				{
					numBackPoints++;
				}
				else if ( distances[i] > 0.0f )
				{
					numFrontPoints++;
				}
			}

			// Non-spanning cases
			if ( numFrontPoints == 0 && numBackPoints == 0 )
			{
				double dot = Plane.Normal.Dot( plane.Normal );

				// Usually the dot product will be 1 or -1 here
				// If it's 1, it means this polygon's plane is really coplanar to
				// the splitting plane, and as such, is the coplanar front plane
				if ( dot >= 0.0f )
				{
					result.CoplanarFront = this;
				}
				else
				{
					result.CoplanarBack = this;
				}

				return result;
			}
			// No back points, all verts are in front of the splitting plane
			else if ( numBackPoints == 0 )
			{
				result.Front = this;
				return result;
			}
			else if ( numFrontPoints == 0 )
			{
				result.Back = this;
				return result;
			}

			// There has been a split, let's actually calculate the two intersection points
			// and split up back & front verts into two polygons
			List<Vector3D> backVerts = new();
			List<Vector3D> frontVerts = new();

			for ( int i = 0; i < Points.Count; i++ )
			{
				int j = (i + 1) % Points.Count;

				// 2 vectors that form an edge
				Vector3D v1 = Points[i], v2 = Points[j];
				double d1 = distances[i], d2 = distances[j];

				if ( d1 <= 0.0 )
				{
					backVerts.Add( v1 );
				}
				if ( d1 >= 0.0 )
				{
					frontVerts.Add( v1 );
				}

				if ( (d1 < 0.0 && d2 > 0.0) || (d2 < 0.0 && d1 > 0.0) )
				{
					// Simple edge-plane intersection formula
					double t = d1 / (d1 - d2);

					double x = v1.X * (1.0 - t) + v2.X * t;
					double y = v1.Y * (1.0 - t) + v2.Y * t;
					double z = v1.Z * (1.0 - t) + v2.Z * t;

					Vector3D intersectionPoint = new( (double)x, (double)y, (double)z );
				
					backVerts.Add( intersectionPoint );
					frontVerts.Add( intersectionPoint );
				}
			}

			result.DidIntersect = true;
			// Before you say "OH NO! O(n^2)!", rest assured that even in
			// the craziest of cases, we won't have >64 verts per polygon
			result.Back = new Polygon3D( backVerts.WithUniqueValuesInRadius( Vector3D.One * 0.125f ) );
			// Now if we were dealing with lots of vertices, which we likely
			// will later on in a custom compiler, I'd use a dictionary
			result.Front = new Polygon3D( frontVerts.WithUniqueValuesInRadius( Vector3D.One * 0.125f ) );

			return result;
		}

		public List<Vector3D> Points = new();
	}
}
