// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Geometry
{
	public struct PolygonSplitResult
	{
		public PolygonSplitResult()
		{

		}

		public static bool operator true( PolygonSplitResult psr )
		{
			return psr.DidIntersect;
		}

		public static bool operator false( PolygonSplitResult psr )
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
		public Polygon3D( List<Vector3> points )
		{
			Points = points;
		}

		public Polygon3D( IEnumerable<Vector3> points )
		{
			Points = points.ToList();
		}

		public Polygon3D( Vector3 a, Vector3 b, Vector3 c )
		{
			Points = new()
			{
				a, b, c
			};
		}

		public Polygon3D( Plane plane, float radius )
		{
			Vector3 direction = plane.GetClosestAxis();
			Vector3 bidirection = direction == Vector3.Up ? Vector3.Left : Vector3.Down;

			Vector3 up = bidirection.Cross( plane.Normal ).Normalized();
			Vector3 right = plane.Normal.Cross( up ).Normalized();

			Vector3 pointOnPlane = plane.GetCenter();

			Vector3[] planePoints = new Vector3[4]
			{
				pointOnPlane + right + up, // Top right
				pointOnPlane - right + up, // Top left
				pointOnPlane - right - up, // Bottom left
				pointOnPlane + right - up // Bottom right
			};

			Vector3 centre = planePoints.Average();

			Points = planePoints.Select( v => (v - centre).Normalized() * radius + centre ).ToList();
		}
		#endregion

		#region Properties
		public Plane Plane => new Plane( Points[0], Points[1], Points[2] );
		public Vector3 Origin
		{
			get
			{
				Vector3 sum = Vector3.Zero;
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
				Plane plane = Plane;
				return Points.TrueForAll( v => plane.HasPoint( v ) );
			}

			return true;
		}

		public void Shift( Vector3 shift )
		{
			for ( int i = 0; i < Points.Count; i++ )
			{
				Points[i] = Points[i] + shift;
			}
		}

		public bool Split( Plane plane, out Polygon3D? back, out Polygon3D? front )
		{
			return Split( plane, out back, out front, out _, out _ );
		}

		public bool Split( Plane plane, out Polygon3D? back, out Polygon3D? front, out Polygon3D? coplanarBack, out Polygon3D? coplanarFront )
		{
			var result = Split( plane );

			back = result.Back;
			front = result.Front;
			coplanarBack = result.CoplanarBack;
			coplanarFront = result.CoplanarFront;

			return result.DidIntersect;
		}

		public PolygonSplitResult Split( Plane plane )
		{
			PolygonSplitResult result = new();

			// Points at negative distances will become part of the "back" polygon,
			// and points at positive distances will become part of the "front" polygon
			var distances = Points.Select( plane.DistanceTo ).ToList();

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
				float dot = Plane.Normal.Dot( plane.Normal );

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
			List<Vector3> backVerts = new();
			List<Vector3> frontVerts = new();

			for ( int i = 0; i < Points.Count; i++ )
			{
				int j = (i + 1) % Points.Count;

				// 2 vectors that form an edge
				Vector3 v1 = Points[i], v2 = Points[j];
				float d1 = distances[i], d2 = distances[j];

				if ( d1 <= 0.0f )
				{
					backVerts.Add( v1 );
				}
				if ( d1 >= 0.0f )
				{
					frontVerts.Add( v1 );
				}

				if ( (d1 < 0.0f && d2 > 0.0f) || (d2 < 0.0f && d1 > 0.0f) )
				{
					// Simple edge-plane intersection formula
					float t = d1 / (d1 - d2);
					Vector3 intersectionPoint = v1 * (1.0f - t) + v2 * t;
				
					backVerts.Add( intersectionPoint );
					frontVerts.Add( intersectionPoint );
				}
			}

			result.DidIntersect = true;
			// Before you say "OH NO! O(n^2)!", rest assured that even in
			// the craziest of cases, we won't have >64 verts per polygon
			result.Back = new Polygon3D( backVerts.WithUniqueValuesInRadius( Vector3.One * 0.125f ) );
			// Now if we were dealing with lots of vertices, which we likely
			// will later on in a custom compiler, I'd use a dictionary
			result.Front = new Polygon3D( frontVerts.WithUniqueValuesInRadius( Vector3.One * 0.125f ) );

			return result;
		}

		public List<Vector3> Points = new();
	}
}
