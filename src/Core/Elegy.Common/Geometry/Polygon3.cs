// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Geometry
{
	/// <summary></summary>
	public struct Polygon3SplitResult
	{
		/// <summary></summary>
		public Polygon3SplitResult()
		{

		}

		/// <summary></summary>
		public static bool operator true( Polygon3SplitResult psr )
		{
			return psr.DidIntersect;
		}

		/// <summary></summary>
		public static bool operator false( Polygon3SplitResult psr )
		{
			return !psr.DidIntersect;
		}

		/// <summary></summary>
		public bool DidIntersect = false;
		/// <summary></summary>
		public Polygon3? Front = null;
		/// <summary></summary>
		public Polygon3? Back = null;
		/// <summary></summary>
		public Polygon3? CoplanarFront = null;
		/// <summary></summary>
		public Polygon3? CoplanarBack = null;
	}

	/// <summary>
	/// 3D polygon with at least 3 vertices
	/// </summary>
	public struct Polygon3
	{
		#region Constructors
		/// <summary></summary>
		public Polygon3( List<Vector3> points )
		{
			Points = points;
		}

		/// <summary></summary>
		public Polygon3( IEnumerable<Vector3> points )
		{
			Points = points.ToList();
		}

		/// <summary></summary>
		public Polygon3( Vector3 a, Vector3 b, Vector3 c )
		{
			Points = new()
			{
				a, b, c
			};
		}

		/// <summary></summary>
		public Polygon3( Plane plane, float radius )
		{
			Vector3 direction = plane.GetClosestAxis();
			Vector3 bidirection = direction == Coords.Up ? Coords.Left : Coords.Down;

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
		/// <summary></summary>
		public Plane Plane => Coords.PlaneFromPoints( Points[0], Points[1], Points[2] );

		/// <summary></summary>
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

		/// <summary></summary>
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

		/// <summary></summary>
		public void Shift( Vector3 shift )
		{
			for ( int i = 0; i < Points.Count; i++ )
			{
				Points[i] = Points[i] + shift;
			}
		}

		/// <summary></summary>
		public bool Split( Plane plane, out Polygon3? back, out Polygon3? front )
		{
			return Split( plane, out back, out front, out _, out _ );
		}

		/// <summary></summary>
		public bool Split( Plane plane, out Polygon3? back, out Polygon3? front, out Polygon3? coplanarBack, out Polygon3? coplanarFront )
		{
			var result = Split( plane );

			back = result.Back;
			front = result.Front;
			coplanarBack = result.CoplanarBack;
			coplanarFront = result.CoplanarFront;

			return result.DidIntersect;
		}

		/// <summary></summary>
		public Polygon3SplitResult Split( Plane plane )
		{
			Polygon3SplitResult result = new();

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

					Vector3 intersectionPoint = new( (float)x, (float)y, (float)z );
				
					backVerts.Add( intersectionPoint );
					frontVerts.Add( intersectionPoint );
				}
			}

			result.DidIntersect = true;
			// Before you say "OH NO! O(n^2)!", rest assured that even in
			// the craziest of cases, we won't have >64 verts per polygon
			result.Back = new Polygon3( backVerts.WithUniqueValuesInRadius( Vector3.One * 0.125f ) );
			// Now if we were dealing with lots of vertices, which we likely
			// will later on in a custom compiler, I'd use a dictionary
			result.Front = new Polygon3( frontVerts.WithUniqueValuesInRadius( Vector3.One * 0.125f ) );

			return result;
		}

		/// <summary></summary>
		public List<Vector3> Points = new();
	}
}
