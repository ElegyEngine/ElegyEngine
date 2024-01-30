
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Elegy.Maths
{
	/// <summary>Represents a plane in three-dimensional space.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// [!INCLUDE[vectors-are-rows-paragraph](~/includes/system-numerics-vectors-are-rows.md)]
	/// ]]></format></remarks>
	public struct PlaneD : IEquatable<PlaneD>
	{
		private const double NormalizeEpsilon = 1.192092896e-07f; // smallest such that 1.0+NormalizeEpsilon != 1.0

		/// <summary>The normal vector of the plane.</summary>
		public Vector3D Normal;

		/// <summary>The distance of the plane along its normal from the origin.</summary>
		public double D;

		/// <summary>Creates a <see cref="PlaneD" /> object from the X, Y, and Z components of its normal, and its distance from the origin on that normal.</summary>
		/// <param name="x">The X component of the normal.</param>
		/// <param name="y">The Y component of the normal.</param>
		/// <param name="z">The Z component of the normal.</param>
		/// <param name="d">The distance of the plane along its normal from the origin.</param>
		public PlaneD( double x, double y, double z, double d )
		{
			Normal = new Vector3D( x, y, z );
			D = d;
		}

		/// <summary>Creates a <see cref="PlaneD" /> object from a specified normal and the distance along the normal from the origin.</summary>
		/// <param name="normal">The plane's normal vector.</param>
		/// <param name="d">The plane's distance from the origin along its normal vector.</param>
		public PlaneD( Vector3D normal, double d )
		{
			Normal = normal;
			D = d;
		}

		/// <summary>Creates a <see cref="PlaneD" /> object from a specified four-dimensional vector.</summary>
		/// <param name="value">A vector whose first three elements describe the normal vector, and whose <see cref="Vector4.W" /> defines the distance along that normal from the origin.</param>
		public PlaneD( Vector4 value )
		{
			Normal = new Vector3D( value.X, value.Y, value.Z );
			D = value.W;
		}

		/// <summary>Creates a <see cref="PlaneD" /> object that contains three specified points.</summary>
		/// <param name="point1">The first point defining the plane.</param>
		/// <param name="point2">The second point defining the plane.</param>
		/// <param name="point3">The third point defining the plane.</param>
		/// <returns>The plane containing the three points.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static PlaneD CreateFromVertices( Vector3D point1, Vector3D point2, Vector3D point3 )
		{
			if ( Vector128.IsHardwareAccelerated )
			{
				Vector3D a = point2 - point1;
				Vector3D b = point3 - point1;

				// N = Cross(a, b)
				Vector3D n = a.Cross( b );
				Vector3D normal = n.Normalized();

				// D = - Dot(N, point1)
				double d = -normal.Dot( point1 );

				return new PlaneD( normal, d );
			}
			else
			{
				double ax = point2.X - point1.X;
				double ay = point2.Y - point1.Y;
				double az = point2.Z - point1.Z;

				double bx = point3.X - point1.X;
				double by = point3.Y - point1.Y;
				double bz = point3.Z - point1.Z;

				// N=Cross(a,b)
				double nx = ay * bz - az * by;
				double ny = az * bx - ax * bz;
				double nz = ax * by - ay * bx;

				// Normalize(N)
				double ls = nx * nx + ny * ny + nz * nz;
				double invNorm = 1.0f / Math.Sqrt( ls );

				Vector3D normal = new Vector3D(
					nx * invNorm,
					ny * invNorm,
					nz * invNorm );

				return new PlaneD(
					normal,
					-(normal.X * point1.X + normal.Y * point1.Y + normal.Z * point1.Z) );
			}
		}

		/// <summary>Calculates the dot product of a plane and a 4-dimensional vector.</summary>
		/// <param name="plane">The plane.</param>
		/// <param name="value">The four-dimensional vector.</param>
		/// <returns>The dot product.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static double Dot( PlaneD plane, Vector4 value )
		{
			return (plane.Normal.X * value.X)
				 + (plane.Normal.Y * value.Y)
				 + (plane.Normal.Z * value.Z)
				 + (plane.D * value.W);
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public double Dot( Vector4 with )
			=> Dot( this, with );
		

		/// <summary>Returns the dot product of a specified three-dimensional vector and the normal vector of this plane plus the distance (<see cref="D" />) value of the plane.</summary>
		/// <param name="plane">The plane.</param>
		/// <param name="value">The 3-dimensional vector.</param>
		/// <returns>The dot product.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static double DotCoordinate( PlaneD plane, Vector3D value )
		{
			if ( Vector128.IsHardwareAccelerated )
			{
				return plane.Normal.Dot( value ) + plane.D;
			}
			else
			{
				return plane.Normal.X * value.X +
					   plane.Normal.Y * value.Y +
					   plane.Normal.Z * value.Z +
					   plane.D;
			}
		}

		/// <summary>Returns the dot product of a specified three-dimensional vector and the <see cref="Normal" /> vector of this plane.</summary>
		/// <param name="plane">The plane.</param>
		/// <param name="value">The three-dimensional vector.</param>
		/// <returns>The dot product.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static double DotNormal( PlaneD plane, Vector3D value )
		{
			if ( Vector128.IsHardwareAccelerated )
			{
				return plane.Normal.Dot( value );
			}
			else
			{
				return plane.Normal.X * value.X +
					   plane.Normal.Y * value.Y +
					   plane.Normal.Z * value.Z;
			}
		}

		/// <summary>Creates a new <see cref="PlaneD" /> object whose normal vector is the source plane's normal vector normalized.</summary>
		/// <param name="value">The source plane.</param>
		/// <returns>The normalized plane.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static PlaneD Normalize( PlaneD value )
		{
			if ( Vector128.IsHardwareAccelerated )
			{
				double normalLengthSquared = value.Normal.LengthSquared();
				if ( Math.Abs( normalLengthSquared - 1.0f ) < NormalizeEpsilon )
				{
					// It already normalized, so we don't need to farther process.
					return value;
				}
				double normalLength = Math.Sqrt( normalLengthSquared );
				return new PlaneD(
					value.Normal / normalLength,
					value.D / normalLength );
			}
			else
			{
				double f = value.Normal.X * value.Normal.X + value.Normal.Y * value.Normal.Y + value.Normal.Z * value.Normal.Z;

				if ( Math.Abs( f - 1.0 ) < NormalizeEpsilon )
				{
					return value; // It already normalized, so we don't need to further process.
				}

				double fInv = 1.0f / Math.Sqrt( f );

				return new PlaneD(
					value.Normal.X * fInv,
					value.Normal.Y * fInv,
					value.Normal.Z * fInv,
					value.D * fInv );
			}
		}

		/// <summary>Transforms a normalized plane by a 4x4 matrix.</summary>
		/// <param name="plane">The normalized plane to transform.</param>
		/// <param name="matrix">The transformation matrix to apply to <paramref name="plane" />.</param>
		/// <returns>The transformed plane.</returns>
		/// <remarks><paramref name="plane" /> must already be normalized so that its <see cref="Normal" /> vector is of unit length before this method is called.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static PlaneD Transform( PlaneD plane, Matrix4x4 matrix )
		{
			Matrix4x4.Invert( matrix, out Matrix4x4 m );

			double x = plane.Normal.X, y = plane.Normal.Y, z = plane.Normal.Z, w = plane.D;

			return new PlaneD(
				x * m.M11 + y * m.M12 + z * m.M13 + w * m.M14,
				x * m.M21 + y * m.M22 + z * m.M23 + w * m.M24,
				x * m.M31 + y * m.M32 + z * m.M33 + w * m.M34,
				x * m.M41 + y * m.M42 + z * m.M43 + w * m.M44 );
		}

		/// <summary>Transforms a normalized plane by a Quaternion rotation.</summary>
		/// <param name="plane">The normalized plane to transform.</param>
		/// <param name="rotation">The Quaternion rotation to apply to the plane.</param>
		/// <returns>A new plane that results from applying the Quaternion rotation.</returns>
		/// <remarks><paramref name="plane" /> must already be normalized so that its <see cref="Normal" /> vector is of unit length before this method is called.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static PlaneD Transform( PlaneD plane, Quaternion rotation )
		{
			// Compute rotation matrix.
			double x2 = rotation.X + rotation.X;
			double y2 = rotation.Y + rotation.Y;
			double z2 = rotation.Z + rotation.Z;

			double wx2 = rotation.W * x2;
			double wy2 = rotation.W * y2;
			double wz2 = rotation.W * z2;
			double xx2 = rotation.X * x2;
			double xy2 = rotation.X * y2;
			double xz2 = rotation.X * z2;
			double yy2 = rotation.Y * y2;
			double yz2 = rotation.Y * z2;
			double zz2 = rotation.Z * z2;

			double m11 = 1.0f - yy2 - zz2;
			double m21 = xy2 - wz2;
			double m31 = xz2 + wy2;

			double m12 = xy2 + wz2;
			double m22 = 1.0f - xx2 - zz2;
			double m32 = yz2 - wx2;

			double m13 = xz2 - wy2;
			double m23 = yz2 + wx2;
			double m33 = 1.0f - xx2 - yy2;

			double x = plane.Normal.X, y = plane.Normal.Y, z = plane.Normal.Z;

			return new PlaneD(
				x * m11 + y * m21 + z * m31,
				x * m12 + y * m22 + z * m32,
				x * m13 + y * m23 + z * m33,
				plane.D );
		}

		/// <summary>
		/// Returns the shortest distance from this plane to the position <paramref name="point"/>.
		/// </summary>
		/// <param name="point">The position to use for the calculation.</param>
		/// <returns>The shortest distance.</returns>
		public readonly double DistanceTo( Vector3D point )
		{
			return Normal.Dot( point ) - D;
		}

		/// <summary>
		/// Returns the center of the plane, the point on the plane closest to the origin.
		/// The point where the normal line going through the origin intersects the plane.
		/// </summary>
		/// <value>Equivalent to <see cref="Plane.Normal"/> multiplied by <see cref="Plane.D"/>.</value>
		public readonly Vector3D GetCenter()
		{
			return Normal * D;
		}

		/// <summary>
		/// Returns <see langword="true"/> if point is inside the plane.
		/// Comparison uses a custom minimum tolerance threshold.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <param name="tolerance">The tolerance threshold.</param>
		/// <returns>A <see langword="bool"/> for whether or not the plane has the point.</returns>
		public readonly bool HasPoint( Vector3D point, double tolerance = double.Epsilon )
		{
			double dist = Normal.Dot( point ) - D;
			return Math.Abs( dist ) <= tolerance;
		}

		/// <summary>
		/// Returns the intersection point of the three planes: <paramref name="b"/>, <paramref name="c"/>,
		/// and this plane. If no intersection is found, <see langword="null"/> is returned.
		/// </summary>
		/// <param name="b">One of the three planes to use in the calculation.</param>
		/// <param name="c">One of the three planes to use in the calculation.</param>
		/// <returns>The intersection, or <see langword="null"/> if none is found.</returns>
		public readonly Vector3D? Intersect3( PlaneD b, PlaneD c )
		{
			double denom = Normal.Cross( b.Normal ).Dot( c.Normal );

			if ( Utils.IsZeroApprox( denom ) )
			{
				return null;
			}

			Vector3D result = (b.Normal.Cross( c.Normal ) * D) +
								(c.Normal.Cross( Normal ) * b.D) +
								(Normal.Cross( b.Normal ) * c.D);

			return result / denom;
		}

		/// <summary>
		/// Returns the intersection point of a ray consisting of the position <paramref name="from"/>
		/// and the direction normal <paramref name="dir"/> with this plane.
		/// If no intersection is found, <see langword="null"/> is returned.
		/// </summary>
		/// <param name="from">The start of the ray.</param>
		/// <param name="dir">The direction of the ray, normalized.</param>
		/// <returns>The intersection, or <see langword="null"/> if none is found.</returns>
		public readonly Vector3D? IntersectsRay( Vector3D from, Vector3D dir )
		{
			double den = Normal.Dot( dir );

			if ( Utils.IsZeroApprox( den ) )
			{
				return null;
			}

			double dist = (Normal.Dot( from ) - D) / den;

			// This is a ray, before the emitting pos (from) does not exist
			if ( dist > double.Epsilon )
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
		/// <param name="begin">The start of the line segment.</param>
		/// <param name="end">The end of the line segment.</param>
		/// <returns>The intersection, or <see langword="null"/> if none is found.</returns>
		public readonly Vector3D? IntersectsSegment( Vector3D begin, Vector3D end )
		{
			Vector3D segment = begin - end;
			double den = Normal.Dot( segment );

			if ( Utils.IsZeroApprox( den ) )
			{
				return null;
			}

			double dist = (Normal.Dot( begin ) - D) / den;

			// Only allow dist to be in the range of 0 to 1, with tolerance.
			if ( dist < -double.Epsilon || dist > 1.0f + double.Epsilon )
			{
				return null;
			}

			return begin - (segment * dist);
		}

		/// <summary>
		/// Returns <see langword="true"/> if this plane is finite, by calling
		/// <see cref="float.IsFinite"/> on each component.
		/// </summary>
		/// <returns>Whether this vector is finite or not.</returns>
		public readonly bool IsFinite()
		{
			return Normal.IsFinite() && double.IsFinite( D );
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="point"/> is located above the plane.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>A <see langword="bool"/> for whether or not the point is above the plane.</returns>
		public readonly bool IsPointOver( Vector3D point )
		{
			return Normal.Dot( point ) > D;
		}

		/// <summary>
		/// Returns the plane scaled to unit length.
		/// </summary>
		/// <returns>A normalized version of the plane.</returns>
		public readonly PlaneD Normalized()
		{
			double len = Normal.Length();

			if ( len == 0 )
			{
				return new PlaneD( 0, 0, 0, 0 );
			}

			return new PlaneD( Normal / len, D / len );
		}

		/// <summary>
		/// Returns the orthogonal projection of <paramref name="point"/> into the plane.
		/// </summary>
		/// <param name="point">The point to project.</param>
		/// <returns>The projected point.</returns>
		public readonly Vector3D Project( Vector3D point )
		{
			return point - (Normal * DistanceTo( point ));
		}

		/// <summary>Returns a value that indicates whether two planes are equal.</summary>
		/// <param name="value1">The first plane to compare.</param>
		/// <param name="value2">The second plane to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="value1" /> and <paramref name="value2" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks>Two <see cref="PlaneD" /> objects are equal if their <see cref="Normal" /> and <see cref="D" /> fields are equal.
		/// The <see cref="op_Equality" /> method defines the operation of the equality operator for <see cref="PlaneD" /> objects.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator ==( PlaneD value1, PlaneD value2 )
		{
			return (value1.Normal == value2.Normal)
				&& (value1.D == value2.D);
		}

		/// <summary>Returns a value that indicates whether two planes are not equal.</summary>
		/// <param name="value1">The first plane to compare.</param>
		/// <param name="value2">The second plane to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="value1" /> and <paramref name="value2" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks>The <see cref="op_Inequality" /> method defines the operation of the inequality operator for <see cref="PlaneD" /> objects.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator !=( PlaneD value1, PlaneD value2 )
		{
			return !(value1 == value2);
		}

		/// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the current instance and <paramref name="obj" /> are equal; otherwise, <see langword="false" />. If <paramref name="obj" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
		/// <remarks>The current instance and <paramref name="obj" /> are equal if <paramref name="obj" /> is a <see cref="PlaneD" /> object and their <see cref="Normal" /> and <see cref="D" /> fields are equal.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override readonly bool Equals( [NotNullWhen( true )] object? obj )
		{
			return (obj is PlaneD other) && Equals( other );
		}

		/// <summary>Returns a value that indicates whether this instance and another plane object are equal.</summary>
		/// <param name="other">The other plane.</param>
		/// <returns><see langword="true" /> if the two planes are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks>Two <see cref="PlaneD" /> objects are equal if their <see cref="Normal" /> and <see cref="D" /> fields are equal.</remarks>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool Equals( PlaneD other )
		{
			// This function needs to account for floating-point equality around NaN
			// and so must behave equivalently to the underlying double/double.Equals

			//if ( Vector128.IsHardwareAccelerated )
			//{
			//	return this.AsVector128().Equals( other.AsVector128() );
			//}

			return SoftwareFallback( in this, other );

			static bool SoftwareFallback( in PlaneD self, PlaneD other )
			{
				return self.Normal.Equals( other.Normal )
					&& self.D.Equals( other.D );
			}
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>The hash code.</returns>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine( Normal, D );
		}

		/// <summary>Returns the string representation of this plane object.</summary>
		/// <returns>A string that represents this <see cref="PlaneD" /> object.</returns>
		/// <remarks>The string representation of a <see cref="PlaneD" /> object use the formatting conventions of the current culture to format the numeric values in the returned string. For example, a <see cref="PlaneD" /> object whose string representation is formatted by using the conventions of the en-US culture might appear as <c>{Normal:&lt;1.1, 2.2, 3.3&gt; D:4.4}</c>.</remarks>
		public override readonly string ToString() => $"{{Normal:{Normal} D:{D}}}";
	}
}
