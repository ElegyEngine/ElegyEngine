// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Vector3.cs

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Elegy.Common.Maths
{
	/// <summary>
	/// 3-element structure that can be used to represent positions in 3D space or any other pair of numeric values.
	/// </summary>
	[Serializable]
	[StructLayout( LayoutKind.Sequential )]
	public struct Vector3D : IEquatable<Vector3D>
	{
		/// <summary>
		/// The vector's X component. Also accessible by using the index position <c>[0]</c>.
		/// </summary>
		public double X;

		/// <summary>
		/// The vector's Y component. Also accessible by using the index position <c>[1]</c>.
		/// </summary>
		public double Y;

		/// <summary>
		/// The vector's Z component. Also accessible by using the index position <c>[2]</c>.
		/// </summary>
		public double Z;

		/// <summary>
		/// Access vector components using their index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not 0, 1 or 2.
		/// </exception>
		/// <value>
		/// <c>[0]</c> is equivalent to <see cref="X"/>,
		/// <c>[1]</c> is equivalent to <see cref="Y"/>,
		/// <c>[2]</c> is equivalent to <see cref="Z"/>.
		/// </value>
		public double this[int index]
		{
			readonly get
			{
				switch ( index )
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					default:
						throw new ArgumentOutOfRangeException( nameof( index ) );
				}
			}
			set
			{
				switch ( index )
				{
					case 0:
						X = value;
						return;
					case 1:
						Y = value;
						return;
					case 2:
						Z = value;
						return;
					default:
						throw new ArgumentOutOfRangeException( nameof( index ) );
				}
			}
		}

		/// <summary>
		/// Helper method for deconstruction into a tuple.
		/// </summary>
		public readonly void Deconstruct( out double x, out double y, out double z )
		{
			x = X;
			y = Y;
			z = Z;
		}

		internal void Normalize()
		{
			double lengthsq = LengthSquared();

			if ( lengthsq == 0 )
			{
				X = Y = Z = 0f;
			}
			else
			{
				double length = Math.Sqrt( lengthsq );
				X /= length;
				Y /= length;
				Z /= length;
			}
		}

		/// <summary>
		/// Returns a new vector with all components in absolute values (i.e. positive).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Abs(double)"/> called on each component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Abs()
		{
			return new Vector3D( Math.Abs( X ), Math.Abs( Y ), Math.Abs( Z ) );
		}

		/// <summary>
		/// Returns the unsigned minimum angle to the given vector, in radians.
		/// </summary>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <returns>The unsigned angle between the two vectors, in radians.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double AngleTo( Vector3D to )
		{
			return Math.Atan2( Cross( to ).Length(), Dot( to ) );
		}

		/// <summary>
		/// Returns a new vector with all components rounded up (towards positive infinity).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Ceiling(double)"/> called on each component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Ceiling()
		{
			return new Vector3D( Math.Ceiling( X ), Math.Ceiling( Y ), Math.Ceiling( Z ) );
		}

		/// <summary>
		/// Returns a new vector with all components clamped between the
		/// components of <paramref name="min"/> and <paramref name="max"/> using
		/// <see cref="Math.Clamp(double, double, double)"/>.
		/// </summary>
		/// <param name="min">The vector with minimum allowed values.</param>
		/// <param name="max">The vector with maximum allowed values.</param>
		/// <returns>The vector with all components clamped.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Clamp( Vector3D min, Vector3D max )
		{
			return new Vector3D
			(
				Math.Clamp( X, min.X, max.X ),
				Math.Clamp( Y, min.Y, max.Y ),
				Math.Clamp( Z, min.Z, max.Z )
			);
		}

		/// <summary>
		/// Returns the cross product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other vector.</param>
		/// <returns>The cross product vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Cross( Vector3D with )
		{
			return new Vector3D
			(
				(Y * with.Z) - (Z * with.Y),
				(Z * with.X) - (X * with.Z),
				(X * with.Y) - (Y * with.X)
			);
		}

		/// <summary>
		/// Performs a cubic interpolation between vectors <paramref name="preA"/>, this vector,
		/// <paramref name="b"/>, and <paramref name="postB"/>, by the given amount <paramref name="weight"/>.
		/// </summary>
		/// <param name="b">The destination vector.</param>
		/// <param name="preA">A vector before this vector.</param>
		/// <param name="postB">A vector after <paramref name="b"/>.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The interpolated vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D CubicInterpolate( Vector3D b, Vector3D preA, Vector3D postB, double weight )
		{
			return new Vector3D
			(
				Utils.Cubic( X, b.X, preA.X, postB.X, weight ),
				Utils.Cubic( Y, b.Y, preA.Y, postB.Y, weight ),
				Utils.Cubic( Z, b.Z, preA.Z, postB.Z, weight )
			);
		}

		/// <summary>
		/// Performs a cubic interpolation between vectors <paramref name="preA"/>, this vector,
		/// <paramref name="b"/>, and <paramref name="postB"/>, by the given amount <paramref name="weight"/>.
		/// It can perform smoother interpolation than <see cref="CubicInterpolate"/>
		/// by the time values.
		/// </summary>
		/// <param name="b">The destination vector.</param>
		/// <param name="preA">A vector before this vector.</param>
		/// <param name="postB">A vector after <paramref name="b"/>.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <param name="t"></param>
		/// <param name="preAT"></param>
		/// <param name="postBT"></param>
		/// <returns>The interpolated vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D CubicInterpolateInTime( Vector3D b, Vector3D preA, Vector3D postB, double weight, double t, double preAT, double postBT )
		{
			return new Vector3D
			(
				Utils.CubicInTime( X, b.X, preA.X, postB.X, weight, t, preAT, postBT ),
				Utils.CubicInTime( Y, b.Y, preA.Y, postB.Y, weight, t, preAT, postBT ),
				Utils.CubicInTime( Z, b.Z, preA.Z, postB.Z, weight, t, preAT, postBT )
			);
		}

		/// <summary>
		/// Returns the point at the given <paramref name="t"/> on a one-dimensional Bezier curve defined by this vector
		/// and the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination vector.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The interpolated vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D BezierInterpolate( Vector3D control1, Vector3D control2, Vector3D end, double t )
		{
			return new Vector3D
			(
				Utils.Bezier( X, control1.X, control2.X, end.X, t ),
				Utils.Bezier( Y, control1.Y, control2.Y, end.Y, t ),
				Utils.Bezier( Z, control1.Z, control2.Z, end.Z, t )
			);
		}

		/// <summary>
		/// Returns the derivative at the given <paramref name="t"/> on the Bezier curve defined by this vector
		/// and the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination value for the interpolation.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D BezierDerivative( Vector3D control1, Vector3D control2, Vector3D end, double t )
		{
			return new Vector3D(
				Utils.BezierDerivative( X, control1.X, control2.X, end.X, t ),
				Utils.BezierDerivative( Y, control1.Y, control2.Y, end.Y, t ),
				Utils.BezierDerivative( Z, control1.Z, control2.Z, end.Z, t )
			);
		}

		/// <summary>
		/// Returns the normalized vector pointing from this vector to <paramref name="to"/>.
		/// </summary>
		/// <param name="to">The other vector to point towards.</param>
		/// <returns>The direction from this vector to <paramref name="to"/>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D DirectionTo( Vector3D to )
		{
			return new Vector3D( to.X - X, to.Y - Y, to.Z - Z ).Normalized();
		}

		/// <summary>
		/// Returns the squared distance between this vector and <paramref name="to"/>.
		/// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
		/// you need to compare vectors or need the squared distance for some formula.
		/// </summary>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double DistanceSquaredTo( Vector3D to )
		{
			return (to - this).LengthSquared();
		}

		/// <summary>
		/// Returns the distance between this vector and <paramref name="to"/>.
		/// </summary>
		/// <seealso cref="DistanceSquaredTo(Vector3D)"/>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The distance between the two vectors.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double DistanceTo( Vector3D to )
		{
			return (to - this).Length();
		}

		/// <summary>
		/// Returns the dot product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other vector to use.</param>
		/// <returns>The dot product of the two vectors.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double Dot( Vector3D with )
		{
			return (X * with.X) + (Y * with.Y) + (Z * with.Z);
		}

		/// <summary>
		/// Returns a new vector with all components rounded down (towards negative infinity).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Floor(double)"/> called on each component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Floor()
		{
			return new Vector3D( Math.Floor( X ), Math.Floor( Y ), Math.Floor( Z ) );
		}

		/// <summary>
		/// Returns the inverse of this vector. This is the same as <c>new Vector3D(1 / v.X, 1 / v.Y, 1 / v.Z)</c>.
		/// </summary>
		/// <returns>The inverse of this vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Inverse()
		{
			return new Vector3D( 1 / X, 1 / Y, 1 / Z );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector is finite, by calling
		/// <see cref="double.IsFinite(double)"/> on each component.
		/// </summary>
		/// <returns>Whether this vector is finite or not.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool IsFinite()
		{
			return double.IsFinite( X ) && double.IsFinite( Y ) && double.IsFinite( Z );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vector is normalized, and <see langword="false"/> otherwise.
		/// </summary>
		/// <returns>A <see langword="bool"/> indicating whether or not the vector is normalized.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool IsNormalized()
		{
			return Math.Abs( LengthSquared() - 1.0f ) < double.Epsilon;
		}

		/// <summary>
		/// Returns the length (magnitude) of this vector.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		/// <returns>The length of this vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double Length()
		{
			double x2 = X * X;
			double y2 = Y * Y;
			double z2 = Z * Z;

			return Math.Sqrt( x2 + y2 + z2 );
		}

		/// <summary>
		/// Returns the squared length (squared magnitude) of this vector.
		/// This method runs faster than <see cref="Length"/>, so prefer it if
		/// you need to compare vectors or need the squared length for some formula.
		/// </summary>
		/// <returns>The squared length of this vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double LengthSquared()
		{
			double x2 = X * X;
			double y2 = Y * Y;
			double z2 = Z * Z;

			return x2 + y2 + z2;
		}

		/// <summary>
		/// Returns the result of the linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		/// </summary>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Lerp( Vector3D to, double weight )
		{
			return new Vector3D
			(
				Utils.Lerp( X, to.X, weight ),
				Utils.Lerp( Y, to.Y, weight ),
				Utils.Lerp( Z, to.Z, weight )
			);
		}

		/// <summary>
		/// Returns the vector with a maximum length by limiting its length to <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The length to limit to.</param>
		/// <returns>The vector with its length limited.</returns>
		public readonly Vector3D LimitLength( double length = 1.0f )
		{
			Vector3D v = this;
			double l = Length();

			if ( l > 0 && length < l )
			{
				v /= l;
				v *= length;
			}

			return v;
		}

		/// <summary>
		/// Returns the axis of the vector's highest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.X"/>.
		/// </summary>
		/// <returns>The index of the highest axis.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Axis MaxAxisIndex()
		{
			return X < Y ? (Y < Z ? Axis.Z : Axis.Y) : (X < Z ? Axis.Z : Axis.X);
		}

		/// <summary>
		/// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.Z"/>.
		/// </summary>
		/// <returns>The index of the lowest axis.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Axis MinAxisIndex()
		{
			return X < Y ? (X < Z ? Axis.X : Axis.Z) : (Y < Z ? Axis.Y : Axis.Z);
		}

		/// <summary>
		/// Moves this vector toward <paramref name="to"/> by the fixed <paramref name="delta"/> amount.
		/// </summary>
		/// <param name="to">The vector to move towards.</param>
		/// <param name="delta">The amount to move towards by.</param>
		/// <returns>The resulting vector.</returns>
		public readonly Vector3D MoveToward( Vector3D to, double delta )
		{
			Vector3D v = this;
			Vector3D vd = to - v;
			double len = vd.Length();
			if ( len <= delta || len < double.Epsilon )
				return to;

			return v + (vd / len * delta);
		}

		/// <summary>
		/// Returns the vector scaled to unit length. Equivalent to <c>v / v.Length()</c>.
		/// </summary>
		/// <returns>A normalized version of the vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Normalized()
		{
			Vector3D v = this;
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Returns this vector projected onto another vector <paramref name="onNormal"/>.
		/// </summary>
		/// <param name="onNormal">The vector to project onto.</param>
		/// <returns>The projected vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Project( Vector3D onNormal )
		{
			return onNormal * (Dot( onNormal ) / onNormal.LengthSquared());
		}

		/// <summary>
		/// Returns this vector reflected from a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="normal">The normal vector defining the plane to reflect from. Must be normalized.</param>
		/// <returns>The reflected vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Reflect( Vector3D normal )
		{
#if DEBUG
			if ( !normal.IsNormalized() )
			{
				throw new ArgumentException( "Argument is not normalized.", nameof( normal ) );
			}
#endif
			return this - (2.0f * Dot( normal ) * normal);
		}

		/// <summary>
		/// Returns this vector with all components rounded to the nearest integer,
		/// with halfway cases rounded towards the nearest multiple of two.
		/// </summary>
		/// <returns>The rounded vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Round()
		{
			return new Vector3D(
				Math.Round( X ),
				Math.Round( Y ),
				Math.Round( Z )
			);
		}

		/// <summary>
		/// Rotates this vector around a given <paramref name="axis"/> vector by <paramref name="angle"/> (in radians).
		/// The <paramref name="axis"/> vector must be a normalized vector. Warning: uses floating-point maths internally.
		/// </summary>
		/// <param name="axis">The vector to rotate around. Must be normalized.</param>
		/// <param name="angle">The angle to rotate by, in radians.</param>
		/// <returns>The rotated vector.</returns>
		public readonly Vector3D Rotated( Vector3D axis, double angle )
		{
#if DEBUG
			if ( !axis.IsNormalized() )
			{
				throw new ArgumentException( "Argument is not normalized.", nameof( axis ) );
			}
#endif
			Vector3 floatV = new( (float)X, (float)Y, (float)Z );
			Vector3 floatAxis = new( (float)axis.X, (float)axis.Y, (float)axis.Z );
			Vector3 floatResult = Vector3.Transform( floatV, Quaternion.CreateFromAxisAngle( floatAxis, (float)angle ) );

			return new( floatResult.X, floatResult.Y, floatResult.Z );
		}

		/// <summary>
		/// Returns a vector with each component set to one or negative one, depending
		/// on the signs of this vector's components, or zero if the component is zero,
		/// by calling <see cref="Math.Sign(double)"/> on each component.
		/// </summary>
		/// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Sign()
		{
			Vector3D v;
			v.X = Math.Sign( X );
			v.Y = Math.Sign( Y );
			v.Z = Math.Sign( Z );
			return v;
		}

		/// <summary>
		/// Returns the signed angle to the given vector, in radians.
		/// The sign of the angle is positive in a counter-clockwise
		/// direction and negative in a clockwise direction when viewed
		/// from the side specified by the <paramref name="axis"/>.
		/// </summary>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <param name="axis">The reference axis to use for the angle sign.</param>
		/// <returns>The signed angle between the two vectors, in radians.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double SignedAngleTo( Vector3D to, Vector3D axis )
		{
			Vector3D crossTo = Cross( to );
			double unsignedAngle = Math.Atan2( crossTo.Length(), Dot( to ) );
			double sign = crossTo.Dot( axis );
			return (sign < 0) ? -unsignedAngle : unsignedAngle;
		}

		/// <summary>
		/// Returns the result of the spherical linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		///
		/// This method also handles interpolating the lengths if the input vectors
		/// have different lengths. For the special case of one or both input vectors
		/// having zero length, this method behaves like <see cref="Lerp(Vector3D, double)"/>.
		/// </summary>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Slerp( Vector3D to, double weight )
		{
			double startLengthSquared = LengthSquared();
			double endLengthSquared = to.LengthSquared();
			if ( startLengthSquared == 0.0 || endLengthSquared == 0.0 )
			{
				// Zero length vectors have no angle, so the best we can do is either lerp or throw an error.
				return Lerp( to, weight );
			}
			double startLength = Math.Sqrt( startLengthSquared );
			double resultLength = Utils.Lerp( startLength, Math.Sqrt( endLengthSquared ), weight );
			double angle = AngleTo( to );
			return Rotated( Cross( to ).Normalized(), angle * weight ) * (resultLength / startLength);
		}

		/// <summary>
		/// Returns this vector slid along a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="normal">The normal vector defining the plane to slide on.</param>
		/// <returns>The slid vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Slide( Vector3D normal )
		{
			return this - (normal * Dot( normal ));
		}

		/// <summary>
		/// Returns this vector with each component snapped to the nearest multiple of <paramref name="step"/>.
		/// This can also be used to round to an arbitrary number of decimals.
		/// </summary>
		/// <param name="step">A vector value representing the step size to snap to.</param>
		/// <returns>The snapped vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector3D Snapped( Vector3D step )
		{
			return new Vector3D
			(
				Utils.Snapped( X, step.X ),
				Utils.Snapped( Y, step.Y ),
				Utils.Snapped( Z, step.Z )
			);
		}

		/// <summary>
		/// Zero vector, a vector with all components set to <c>0</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector3D(0, 0, 0)</c>.</value>
		public static Vector3D Zero => new( 0.0, 0.0, 0.0 );
		/// <summary>
		/// One vector, a vector with all components set to <c>1</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector3D(1, 1, 1)</c>.</value>
		public static Vector3D One => new( 1.0, 1.0, 1.0 );
		/// <summary>
		/// Infinity vector, a vector with all components set to <see cref="double.PositiveInfinity"/>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector3D(Math.Inf, Math.Inf, Math.Inf)</c>.</value>
		public static Vector3D Inf => new( double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity );

		/// <summary>
		/// Unit vector along the X axis: <c>(1 0 0)</c>.
		/// </summary>
		public static Vector3D UnitX => new( 1.0, 0.0, 0.0 );
		/// <summary>
		/// Unit vector along the Y axis: <c>(0 1 0)</c>.
		/// </summary>
		public static Vector3D UnitY => new( 0.0, 1.0, 0.0 );
		/// <summary>
		/// Unit vector along the Z axis: <c>(0 0 1)</c>.
		/// </summary>
		public static Vector3D UnitZ => new( 0.0, 0.0, 1.0 );

		/// <summary>
		/// Constructs a new <see cref="Vector3D"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		/// <param name="z">The vector's Z component.</param>
		public Vector3D( int x, int y, int z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Constructs a new <see cref="Vector3D"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		/// <param name="z">The vector's Z component.</param>
		public Vector3D( float x, float y, float z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Constructs a new <see cref="Vector3D"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		/// <param name="z">The vector's Z component.</param>
		public Vector3D( double x, double y, double z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Adds each component of the <see cref="Vector3D"/>
		/// with the components of the given <see cref="Vector3D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The added vector.</returns>
		public static Vector3D operator +( Vector3D left, Vector3D right )
		{
			left.X += right.X;
			left.Y += right.Y;
			left.Z += right.Z;
			return left;
		}

		/// <summary>
		/// Subtracts each component of the <see cref="Vector3D"/>
		/// by the components of the given <see cref="Vector3D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The subtracted vector.</returns>
		public static Vector3D operator -( Vector3D left, Vector3D right )
		{
			left.X -= right.X;
			left.Y -= right.Y;
			left.Z -= right.Z;
			return left;
		}

		/// <summary>
		/// Returns the negative value of the <see cref="Vector3D"/>.
		/// This is the same as writing <c>new Vector3D(-v.X, -v.Y, -v.Z)</c>.
		/// This operation flips the direction of the vector while
		/// keeping the same magnitude.
		/// With floats, the number zero can be either positive or negative.
		/// </summary>
		/// <param name="vec">The vector to negate/flip.</param>
		/// <returns>The negated/flipped vector.</returns>
		public static Vector3D operator -( Vector3D vec )
		{
			vec.X = -vec.X;
			vec.Y = -vec.Y;
			vec.Z = -vec.Z;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector3D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="vec">The vector to multiply.</param>
		/// <param name="scale">The scale to multiply by.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector3D operator *( Vector3D vec, double scale )
		{
			vec.X *= scale;
			vec.Y *= scale;
			vec.Z *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector3D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="scale">The scale to multiply by.</param>
		/// <param name="vec">The vector to multiply.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector3D operator *( double scale, Vector3D vec )
		{
			vec.X *= scale;
			vec.Y *= scale;
			vec.Z *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector3D"/>
		/// by the components of the given <see cref="Vector3D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector3D operator *( Vector3D left, Vector3D right )
		{
			left.X *= right.X;
			left.Y *= right.Y;
			left.Z *= right.Z;
			return left;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector3D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The divided vector.</returns>
		public static Vector3D operator /( Vector3D vec, double divisor )
		{
			vec.X /= divisor;
			vec.Y /= divisor;
			vec.Z /= divisor;
			return vec;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector3D"/>
		/// by the components of the given <see cref="Vector3D"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The divided vector.</returns>
		public static Vector3D operator /( Vector3D vec, Vector3D divisorv )
		{
			vec.X /= divisorv.X;
			vec.Y /= divisorv.Y;
			vec.Z /= divisorv.Z;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector3D"/>
		/// with the components of the given <see cref="double"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// Consider using <see cref="PosMod(double)"/> instead
		/// if you want to handle negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector3D(10, -20, 30) % 7); // Prints "(3, -6, 2)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector3D operator %( Vector3D vec, double divisor )
		{
			vec.X %= divisor;
			vec.Y %= divisor;
			vec.Z %= divisor;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector3D"/>
		/// with the components of the given <see cref="Vector3D"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// Consider using <see cref="PosMod(Vector3D)"/> instead
		/// if you want to handle negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector3D(10, -20, 30) % new Vector3D(7, 8, 9)); // Prints "(3, -4, 3)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector3D operator %( Vector3D vec, Vector3D divisorv )
		{
			vec.X %= divisorv.X;
			vec.Y %= divisorv.Y;
			vec.Z %= divisorv.Z;
			return vec;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are exactly equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the vectors are exactly equal.</returns>
		public static bool operator ==( Vector3D left, Vector3D right )
		{
			return left.Equals( right );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are not equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the vectors are not equal.</returns>
		public static bool operator !=( Vector3D left, Vector3D right )
		{
			return !left.Equals( right );
		}

		/// <summary>
		/// Compares two <see cref="Vector3D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors, and then with the Z values.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than the right.</returns>
		public static bool operator <( Vector3D left, Vector3D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					return left.Z < right.Z;
				}
				return left.Y < right.Y;
			}
			return left.X < right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector3D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors, and then with the Z values.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than the right.</returns>
		public static bool operator >( Vector3D left, Vector3D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					return left.Z > right.Z;
				}
				return left.Y > right.Y;
			}
			return left.X > right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector3D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors, and then with the Z values.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than or equal to the right.</returns>
		public static bool operator <=( Vector3D left, Vector3D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					return left.Z <= right.Z;
				}
				return left.Y < right.Y;
			}
			return left.X < right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector3D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors, and then with the Z values.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than or equal to the right.</returns>
		public static bool operator >=( Vector3D left, Vector3D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					return left.Z >= right.Z;
				}
				return left.Y > right.Y;
			}
			return left.X > right.X;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vector is exactly equal
		/// to the given object (<paramref name="obj"/>).
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Whether or not the vector and the object are equal.</returns>
		public override readonly bool Equals( [NotNullWhen( true )] object? obj )
		{
			return obj is Vector3D other && Equals( other );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are exactly equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="other">The other vector.</param>
		/// <returns>Whether or not the vectors are exactly equal.</returns>
		public readonly bool Equals( Vector3D other )
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Utils.IsEqualApprox(double, double)"/> on each component.
		/// </summary>
		/// <param name="other">The other vector to compare.</param>
		/// <returns>Whether or not the vectors are approximately equal.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool IsEqualApprox( Vector3D other )
		{
			return Utils.IsEqualApprox( X, other.X )
				&& Utils.IsEqualApprox( Y, other.Y )
				&& Utils.IsEqualApprox( Z, other.Z );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector's values are approximately zero,
		/// by running <see cref="Utils.IsZeroApprox(double)"/> on each component.
		/// This method is faster than using <see cref="IsEqualApprox"/> with one value
		/// as a zero vector.
		/// </summary>
		/// <returns>Whether or not the vector is approximately zero.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool IsZeroApprox()
		{
			return Utils.IsZeroApprox( X )
				&& Utils.IsZeroApprox( Y )
				&& Utils.IsZeroApprox( Z );
		}

		/// <summary>
		/// Serves as the hash function for <see cref="Vector3D"/>.
		/// </summary>
		/// <returns>A hash code for this vector.</returns>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine( X, Y, Z );
		}

		/// <summary>
		/// Converts this <see cref="Vector3D"/> to a string.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public override readonly string ToString()
		{
			return $"({X}, {Y}, {Z})";
		}

		/// <summary>
		/// Converts this <see cref="Vector3D"/> to a string with the given <paramref name="format"/>.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public readonly string ToString( string? format )
		{
			return $"({X.ToString( format )}, {Y.ToString( format )}, {Z.ToString( format )})";
		}
	}
}
