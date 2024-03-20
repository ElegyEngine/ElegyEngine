// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Vector2.cs

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Elegy.Common.Maths
{
	/// <summary>
	/// 2-element structure that can be used to represent positions in 2D space or any other pair of numeric values.
	/// </summary>
	[Serializable]
	[StructLayout( LayoutKind.Sequential )]
	public struct Vector2D : IEquatable<Vector2D>
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
		/// Access vector components using their index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not 0 or 1.
		/// </exception>
		/// <value>
		/// <c>[0]</c> is equivalent to <see cref="X"/>,
		/// <c>[1]</c> is equivalent to <see cref="Y"/>.
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
					default:
						throw new ArgumentOutOfRangeException( nameof( index ) );
				}
			}
		}

		/// <summary>
		/// Helper method for deconstruction into a tuple.
		/// </summary>
		public readonly void Deconstruct( out double x, out double y )
		{
			x = X;
			y = Y;
		}

		internal void Normalize()
		{
			double lengthsq = LengthSquared();

			if ( lengthsq == 0 )
			{
				X = Y = 0f;
			}
			else
			{
				double length = Math.Sqrt( lengthsq );
				X /= length;
				Y /= length;
			}
		}

		/// <summary>
		/// Returns a new vector with all components in absolute values (i.e. positive).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Abs(double)"/> called on each component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Abs()
		{
			return new Vector2D( Math.Abs( X ), Math.Abs( Y ) );
		}

		/// <summary>
		/// Returns this vector's angle with respect to the X axis, or (1, 0) vector, in radians.
		///
		/// Equivalent to the result of <see cref="Math.Atan2(double, double)"/> when
		/// called with the vector's <see cref="Y"/> and <see cref="X"/> as parameters: <c>Math.Atan2(v.Y, v.X)</c>.
		/// </summary>
		/// <returns>The angle of this vector, in radians.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double Angle()
		{
			return Math.Atan2( Y, X );
		}

		/// <summary>
		/// Returns the angle to the given vector, in radians.
		/// </summary>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <returns>The angle between the two vectors, in radians.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double AngleTo( Vector2D to )
		{
			return Math.Atan2( Cross( to ), Dot( to ) );
		}

		/// <summary>
		/// Returns the angle between the line connecting the two points and the X axis, in radians.
		/// </summary>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <returns>The angle between the two vectors, in radians.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double AngleToPoint( Vector2D to )
		{
			return Math.Atan2( to.Y - Y, to.X - X );
		}

		/// <summary>
		/// Returns the aspect ratio of this vector, the ratio of <see cref="X"/> to <see cref="Y"/>.
		/// </summary>
		/// <returns>The <see cref="X"/> component divided by the <see cref="Y"/> component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double Aspect()
		{
			return X / Y;
		}

		/// <summary>
		/// Returns a new vector with all components rounded up (towards positive infinity).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Ceiling(double)"/> called on each component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Ceil()
		{
			return new Vector2D( Math.Ceiling( X ), Math.Ceiling( Y ) );
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
		public readonly Vector2D Clamp( Vector2D min, Vector2D max )
		{
			return new Vector2D
			(
				Math.Clamp( X, min.X, max.X ),
				Math.Clamp( Y, min.Y, max.Y )
			);
		}

		/// <summary>
		/// Returns the cross product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other vector.</param>
		/// <returns>The cross product value.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double Cross( Vector2D with )
		{
			return (X * with.Y) - (Y * with.X);
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
		public readonly Vector2D CubicInterpolate( Vector2D b, Vector2D preA, Vector2D postB, double weight )
		{
			return new Vector2D
			(
				Utils.Cubic( X, b.X, preA.X, postB.X, weight ),
				Utils.Cubic( Y, b.Y, preA.Y, postB.Y, weight )
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
		public readonly Vector2D CubicInterpolateInTime( Vector2D b, Vector2D preA, Vector2D postB, double weight, double t, double preAT, double postBT )
		{
			return new Vector2D
			(
				Utils.CubicInTime( X, b.X, preA.X, postB.X, weight, t, preAT, postBT ),
				Utils.CubicInTime( Y, b.Y, preA.Y, postB.Y, weight, t, preAT, postBT )
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
		public readonly Vector2D BezierInterpolate( Vector2D control1, Vector2D control2, Vector2D end, double t )
		{
			return new Vector2D
			(
				Utils.Bezier( X, control1.X, control2.X, end.X, t ),
				Utils.Bezier( Y, control1.Y, control2.Y, end.Y, t )
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
		public readonly Vector2D BezierDerivative( Vector2D control1, Vector2D control2, Vector2D end, double t )
		{
			return new Vector2D(
				Utils.BezierDerivative( X, control1.X, control2.X, end.X, t ),
				Utils.BezierDerivative( Y, control1.Y, control2.Y, end.Y, t )
			);
		}

		/// <summary>
		/// Returns the normalized vector pointing from this vector to <paramref name="to"/>.
		/// </summary>
		/// <param name="to">The other vector to point towards.</param>
		/// <returns>The direction from this vector to <paramref name="to"/>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D DirectionTo( Vector2D to )
		{
			return new Vector2D( to.X - X, to.Y - Y ).Normalized();
		}

		/// <summary>
		/// Returns the squared distance between this vector and <paramref name="to"/>.
		/// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
		/// you need to compare vectors or need the squared distance for some formula.
		/// </summary>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double DistanceSquaredTo( Vector2D to )
		{
			return (X - to.X) * (X - to.X) + (Y - to.Y) * (Y - to.Y);
		}

		/// <summary>
		/// Returns the distance between this vector and <paramref name="to"/>.
		/// </summary>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The distance between the two vectors.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double DistanceTo( Vector2D to )
		{
			return Math.Sqrt( (X - to.X) * (X - to.X) + (Y - to.Y) * (Y - to.Y) );
		}

		/// <summary>
		/// Returns the dot product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other vector to use.</param>
		/// <returns>The dot product of the two vectors.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly double Dot( Vector2D with )
		{
			return (X * with.X) + (Y * with.Y);
		}

		/// <summary>
		/// Returns a new vector with all components rounded down (towards negative infinity).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Floor(double)"/> called on each component.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Floor()
		{
			return new Vector2D( Math.Floor( X ), Math.Floor( Y ) );
		}

		/// <summary>
		/// Returns the inverse of this vector. This is the same as <c>new Vector2D(1 / v.X, 1 / v.Y)</c>.
		/// </summary>
		/// <returns>The inverse of this vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Inverse()
		{
			return new Vector2D( 1 / X, 1 / Y );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector is finite, by calling
		/// <see cref="double.IsFinite(double)"/> on each component.
		/// </summary>
		/// <returns>Whether this vector is finite or not.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool IsFinite()
		{
			return double.IsFinite( X ) && double.IsFinite( Y );
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
			return Math.Sqrt( (X * X) + (Y * Y) );
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
			return (X * X) + (Y * Y);
		}

		/// <summary>
		/// Returns the result of the linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		/// </summary>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Lerp( Vector2D to, double weight )
		{
			return new Vector2D
			(
				Utils.Lerp( X, to.X, weight ),
				Utils.Lerp( Y, to.Y, weight )
			);
		}

		/// <summary>
		/// Returns the vector with a maximum length by limiting its length to <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The length to limit to.</param>
		/// <returns>The vector with its length limited.</returns>
		public readonly Vector2D LimitLength( double length = 1.0f )
		{
			Vector2D v = this;
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
		/// If both components are equal, this method returns <see cref="Axis.X"/>.
		/// </summary>
		/// <returns>The index of the highest axis.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Axis MaxAxisIndex()
		{
			return X < Y ? Axis.Y : Axis.X;
		}

		/// <summary>
		/// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
		/// If both components are equal, this method returns <see cref="Axis.Y"/>.
		/// </summary>
		/// <returns>The index of the lowest axis.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Axis MinAxisIndex()
		{
			return X < Y ? Axis.X : Axis.Y;
		}

		/// <summary>
		/// Moves this vector toward <paramref name="to"/> by the fixed <paramref name="delta"/> amount.
		/// </summary>
		/// <param name="to">The vector to move towards.</param>
		/// <param name="delta">The amount to move towards by.</param>
		/// <returns>The resulting vector.</returns>
		public readonly Vector2D MoveToward( Vector2D to, double delta )
		{
			Vector2D v = this;
			Vector2D vd = to - v;
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
		public readonly Vector2D Normalized()
		{
			Vector2D v = this;
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Returns this vector projected onto another vector <paramref name="onNormal"/>.
		/// </summary>
		/// <param name="onNormal">The vector to project onto.</param>
		/// <returns>The projected vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Project( Vector2D onNormal )
		{
			return onNormal * (Dot( onNormal ) / onNormal.LengthSquared());
		}

		/// <summary>
		/// Returns this vector reflected from a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="normal">The normal vector defining the plane to reflect from. Must be normalized.</param>
		/// <returns>The reflected vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Reflect( Vector2D normal )
		{
#if DEBUG
			if ( !normal.IsNormalized() )
			{
				throw new ArgumentException( "Argument is not normalized.", nameof( normal ) );
			}
#endif
			return this - (2 * Dot( normal ) * normal);
		}

		/// <summary>
		/// Rotates this vector by <paramref name="angle"/> radians.
		/// </summary>
		/// <param name="angle">The angle to rotate by, in radians.</param>
		/// <returns>The rotated vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Rotated( double angle )
		{
			(double sin, double cos) = Math.SinCos( angle );
			return new Vector2D
			(
				X * cos - Y * sin,
				X * sin + Y * cos
			);
		}

		/// <summary>
		/// Returns this vector with all components rounded to the nearest integer,
		/// with halfway cases rounded towards the nearest multiple of two.
		/// </summary>
		/// <returns>The rounded vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Round()
		{
			return new Vector2D( Math.Round( X ), Math.Round( Y ) );
		}

		/// <summary>
		/// Returns a vector with each component set to one or negative one, depending
		/// on the signs of this vector's components, or zero if the component is zero,
		/// by calling <see cref="Math.Sign(double)"/> on each component.
		/// </summary>
		/// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Sign()
		{
			Vector2D v;
			v.X = Math.Sign( X );
			v.Y = Math.Sign( Y );
			return v;
		}

		/// <summary>
		/// Returns the result of the spherical linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		///
		/// This method also handles interpolating the lengths if the input vectors
		/// have different lengths. For the special case of one or both input vectors
		/// having zero length, this method behaves like <see cref="Lerp(Vector2D, double)"/>.
		/// </summary>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		public readonly Vector2D Slerp( Vector2D to, double weight )
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
			return Rotated( angle * weight ) * (resultLength / startLength);
		}

		/// <summary>
		/// Returns this vector slid along a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="normal">The normal vector defining the plane to slide on.</param>
		/// <returns>The slid vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Slide( Vector2D normal )
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
		public readonly Vector2D Snapped( Vector2D step )
		{
			return new Vector2D( Utils.Snapped( X, step.X ), Utils.Snapped( Y, step.Y ) );
		}

		/// <summary>
		/// Returns a perpendicular vector rotated 90 degrees counter-clockwise
		/// compared to the original, with the same length.
		/// </summary>
		/// <returns>The perpendicular vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly Vector2D Orthogonal()
		{
			return new Vector2D( Y, -X );
		}

		/// <summary>
		/// Zero vector, a vector with all components set to <c>0</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector2D(0, 0)</c>.</value>
		public static Vector2D Zero => new( 0.0, 0.0 );
		/// <summary>
		/// One vector, a vector with all components set to <c>1</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector2D(1, 1)</c>.</value>
		public static Vector2D One => new( 1.0, 1.0 );
		/// <summary>
		/// Infinity vector, a vector with all components set to <see cref="double.PositiveInfinity"/>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector2D(Math.Inf, Math.Inf)</c>.</value>
		public static Vector2D Inf => new( double.PositiveInfinity, double.PositiveInfinity );

		/// <summary>
		/// Vector that points +X.
		/// </summary>
		/// <value>Equivalent to <c>new Vector2D(0, -1)</c>.</value>
		public static Vector2D UnitX => new( 1.0, 0.0 );
		/// <summary>
		/// Vector that points +Y.
		/// </summary>
		/// <value>Equivalent to <c>new Vector2D(0, 1)</c>.</value>
		public static Vector2D UnitY => new( 0.0, 1.0 );

		/// <summary>
		/// Constructs a new <see cref="Vector2D"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		public Vector2D( double x, double y )
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Creates a unit Vector2D rotated to the given angle. This is equivalent to doing
		/// <c>Vector2D(Math.Cos(angle), Math.Sin(angle))</c> or <c>Vector2D.Right.Rotated(angle)</c>.
		/// </summary>
		/// <param name="angle">Angle of the vector, in radians.</param>
		/// <returns>The resulting vector.</returns>
		public static Vector2D FromAngle( double angle )
		{
			(double sin, double cos) = Math.SinCos( angle );
			return new Vector2D( cos, sin );
		}

		/// <summary>
		/// Adds each component of the <see cref="Vector2D"/>
		/// with the components of the given <see cref="Vector2D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The added vector.</returns>
		public static Vector2D operator +( Vector2D left, Vector2D right )
		{
			left.X += right.X;
			left.Y += right.Y;
			return left;
		}

		/// <summary>
		/// Subtracts each component of the <see cref="Vector2D"/>
		/// by the components of the given <see cref="Vector2D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The subtracted vector.</returns>
		public static Vector2D operator -( Vector2D left, Vector2D right )
		{
			left.X -= right.X;
			left.Y -= right.Y;
			return left;
		}

		/// <summary>
		/// Returns the negative value of the <see cref="Vector2D"/>.
		/// This is the same as writing <c>new Vector2D(-v.X, -v.Y)</c>.
		/// This operation flips the direction of the vector while
		/// keeping the same magnitude.
		/// With floats, the number zero can be either positive or negative.
		/// </summary>
		/// <param name="vec">The vector to negate/flip.</param>
		/// <returns>The negated/flipped vector.</returns>
		public static Vector2D operator -( Vector2D vec )
		{
			vec.X = -vec.X;
			vec.Y = -vec.Y;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector2D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="vec">The vector to multiply.</param>
		/// <param name="scale">The scale to multiply by.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector2D operator *( Vector2D vec, double scale )
		{
			vec.X *= scale;
			vec.Y *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector2D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="scale">The scale to multiply by.</param>
		/// <param name="vec">The vector to multiply.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector2D operator *( double scale, Vector2D vec )
		{
			vec.X *= scale;
			vec.Y *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector2D"/>
		/// by the components of the given <see cref="Vector2D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector2D operator *( Vector2D left, Vector2D right )
		{
			left.X *= right.X;
			left.Y *= right.Y;
			return left;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector2D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The divided vector.</returns>
		public static Vector2D operator /( Vector2D vec, double divisor )
		{
			vec.X /= divisor;
			vec.Y /= divisor;
			return vec;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector2D"/>
		/// by the components of the given <see cref="Vector2D"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The divided vector.</returns>
		public static Vector2D operator /( Vector2D vec, Vector2D divisorv )
		{
			vec.X /= divisorv.X;
			vec.Y /= divisorv.Y;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector2D"/>
		/// with the components of the given <see cref="double"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector2D(10, -20) % 7); // Prints "(3, -6)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector2D operator %( Vector2D vec, double divisor )
		{
			vec.X %= divisor;
			vec.Y %= divisor;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector2D"/>
		/// with the components of the given <see cref="Vector2D"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector2D(10, -20) % new Vector2D(7, 8)); // Prints "(3, -4)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector2D operator %( Vector2D vec, Vector2D divisorv )
		{
			vec.X %= divisorv.X;
			vec.Y %= divisorv.Y;
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
		public static bool operator ==( Vector2D left, Vector2D right )
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
		public static bool operator !=( Vector2D left, Vector2D right )
		{
			return !left.Equals( right );
		}

		/// <summary>
		/// Compares two <see cref="Vector2D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than the right.</returns>
		public static bool operator <( Vector2D left, Vector2D right )
		{
			if ( left.X == right.X )
			{
				return left.Y < right.Y;
			}
			return left.X < right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector2D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than the right.</returns>
		public static bool operator >( Vector2D left, Vector2D right )
		{
			if ( left.X == right.X )
			{
				return left.Y > right.Y;
			}
			return left.X > right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector2D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than or equal to the right.</returns>
		public static bool operator <=( Vector2D left, Vector2D right )
		{
			if ( left.X == right.X )
			{
				return left.Y <= right.Y;
			}
			return left.X < right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector2D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than or equal to the right.</returns>
		public static bool operator >=( Vector2D left, Vector2D right )
		{
			if ( left.X == right.X )
			{
				return left.Y >= right.Y;
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
			return obj is Vector2D other && Equals( other );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are exactly equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="other">The other vector.</param>
		/// <returns>Whether or not the vectors are exactly equal.</returns>
		public readonly bool Equals( Vector2D other )
		{
			return X == other.X && Y == other.Y;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Utils.IsEqualApprox(double, double)"/> on each component.
		/// </summary>
		/// <param name="other">The other vector to compare.</param>
		/// <returns>Whether or not the vectors are approximately equal.</returns>
		public readonly bool IsEqualApprox( Vector2D other )
		{
			return Utils.IsEqualApprox( X, other.X ) && Utils.IsEqualApprox( Y, other.Y );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector's values are approximately zero,
		/// by running <see cref="Utils.IsZeroApprox(double)"/> on each component.
		/// This method is faster than using <see cref="IsEqualApprox"/> with one value
		/// as a zero vector.
		/// </summary>
		/// <returns>Whether or not the vector is approximately zero.</returns>
		public readonly bool IsZeroApprox()
		{
			return Utils.IsZeroApprox( X ) && Utils.IsZeroApprox( Y );
		}

		/// <summary>
		/// Serves as the hash function for <see cref="Vector2D"/>.
		/// </summary>
		/// <returns>A hash code for this vector.</returns>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine( X, Y );
		}

		/// <summary>
		/// Converts this <see cref="Vector2D"/> to a string.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public override readonly string ToString()
		{
			return $"({X}, {Y})";
		}

		/// <summary>
		/// Converts this <see cref="Vector2D"/> to a string with the given <paramref name="format"/>.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public readonly string ToString( string? format )
		{
			return $"({X.ToString( format )}, {Y.ToString( format )})";
		}
	}
}
