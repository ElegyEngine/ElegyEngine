// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Plane.cs

using Elegy.Maths;

namespace Elegy.Extensions
{
	public static class PlaneGodotExtensions
	{
		/// <summary>
		/// Returns the shortest distance from this plane to the position <paramref name="point"/>.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="point">The position to use for the calculation.</param>
		/// <returns>The shortest distance.</returns>
		public static float DistanceTo( this Plane p, Vector3 point )
		{
			return p.Normal.Dot( point ) - p.D;
		}

		/// <summary>
		/// Returns the center of the plane, the point on the plane closest to the origin.
		/// The point where the normal line going through the origin intersects the plane.
		/// </summary>
		/// <param name="p"></param>
		/// <value>Equivalent to <see cref="Plane.Normal"/> multiplied by <see cref="Plane.D"/>.</value>
		public static Vector3 GetCenter( this Plane p )
		{
			return p.Normal * p.D;
		}

		/// <summary>
		/// Returns <see langword="true"/> if point is inside the plane.
		/// Comparison uses a custom minimum tolerance threshold.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="point">The point to check.</param>
		/// <param name="tolerance">The tolerance threshold.</param>
		/// <returns>A <see langword="bool"/> for whether or not the plane has the point.</returns>
		public static bool HasPoint( this Plane p, Vector3 point, float tolerance = float.Epsilon )
		{
			float dist = p.Normal.Dot( point ) - p.D;
			return MathF.Abs( dist ) <= tolerance;
		}

		/// <summary>
		/// Returns the intersection point of the three planes: <paramref name="b"/>, <paramref name="c"/>,
		/// and this plane. If no intersection is found, <see langword="null"/> is returned.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="b">One of the three planes to use in the calculation.</param>
		/// <param name="c">One of the three planes to use in the calculation.</param>
		/// <returns>The intersection, or <see langword="null"/> if none is found.</returns>
		public static Vector3? Intersect3( this Plane p, Plane b, Plane c )
		{
			float denom = p.Normal.Cross( b.Normal ).Dot( c.Normal );

			if ( Utils.IsZeroApprox( denom ) )
			{
				return null;
			}

			Vector3 result = (b.Normal.Cross( c.Normal ) * p.D) +
								(c.Normal.Cross( p.Normal ) * b.D) +
								(p.Normal.Cross( b.Normal ) * c.D);

			return result / denom;
		}

		/// <summary>
		/// Returns the intersection point of a ray consisting of the position <paramref name="from"/>
		/// and the direction normal <paramref name="dir"/> with this plane.
		/// If no intersection is found, <see langword="null"/> is returned.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="from">The start of the ray.</param>
		/// <param name="dir">The direction of the ray, normalized.</param>
		/// <returns>The intersection, or <see langword="null"/> if none is found.</returns>
		public static Vector3? IntersectsRay( this Plane p, Vector3 from, Vector3 dir )
		{
			float den = p.Normal.Dot( dir );

			if ( Utils.IsZeroApprox( den ) )
			{
				return null;
			}

			float dist = (p.Normal.Dot( from ) - p.D) / den;

			// This is a ray, before the emitting pos (from) does not exist
			if ( dist > float.Epsilon )
			{
				return null;
			}

			return from - (dir * dist);
		}

		/// <summary>
		/// Returns the intersection point of a line segment from
		/// position <paramref name="begin"/> to position <paramref name="end"/> with this plane.
		/// If no intersection is found, <see langword="null"/> is returned.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="begin">The start of the line segment.</param>
		/// <param name="end">The end of the line segment.</param>
		/// <returns>The intersection, or <see langword="null"/> if none is found.</returns>
		public static Vector3? IntersectsSegment( this Plane p, Vector3 begin, Vector3 end )
		{
			Vector3 segment = begin - end;
			float den = p.Normal.Dot( segment );

			if ( Utils.IsZeroApprox( den ) )
			{
				return null;
			}

			float dist = (p.Normal.Dot( begin ) - p.D) / den;

			// Only allow dist to be in the range of 0 to 1, with tolerance.
			if ( dist < -float.Epsilon || dist > 1.0f + float.Epsilon )
			{
				return null;
			}

			return begin - (segment * dist);
		}

		/// <summary>
		/// Returns <see langword="true"/> if this plane is finite, by calling
		/// <see cref="float.IsFinite"/> on each component.
		/// </summary>
		/// <param name="p"></param>
		/// <returns>Whether this vector is finite or not.</returns>
		public static bool IsFinite( this Plane p )
		{
			return p.Normal.IsFinite() && float.IsFinite( p.D );
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="point"/> is located above the plane.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="point">The point to check.</param>
		/// <returns>A <see langword="bool"/> for whether or not the point is above the plane.</returns>
		public static bool IsPointOver( this Plane p, Vector3 point )
		{
			return p.Normal.Dot( point ) > p.D;
		}

		/// <summary>
		/// Returns the plane scaled to unit length.
		/// </summary>
		/// <param name="p"></param>
		/// <returns>A normalized version of the plane.</returns>
		public static Plane Normalized( this Plane p )
		{
			float len = p.Normal.Length();

			if ( len == 0 )
			{
				return new Plane( 0, 0, 0, 0 );
			}

			return new Plane( p.Normal / len, p.D / len );
		}

		/// <summary>
		/// Returns the orthogonal projection of <paramref name="point"/> into the plane.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="point">The point to project.</param>
		/// <returns>The projected point.</returns>
		public static Vector3 Project( this Plane p, Vector3 point )
		{
			return point - (p.Normal * p.DistanceTo( point ));
		}
	}
}
