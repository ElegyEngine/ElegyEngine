// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Vector4D.cs

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Elegy.Common.Maths
{
	/// <summary>
	/// 4-element structure that can be used to represent positions in 4D space or any other pair of numeric values.
	/// </summary>
	[Serializable]
	[StructLayout( LayoutKind.Sequential )]
	public struct Vector4D : IEquatable<Vector4D>
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
		/// The vector's W component. Also accessible by using the index position <c>[3]</c>.
		/// </summary>
		public double W;

		/// <summary>
		/// Access vector components using their index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not 0, 1, 2 or 3.
		/// </exception>
		/// <value>
		/// <c>[0]</c> is equivalent to <see cref="X"/>,
		/// <c>[1]</c> is equivalent to <see cref="Y"/>,
		/// <c>[2]</c> is equivalent to <see cref="Z"/>.
		/// <c>[3]</c> is equivalent to <see cref="W"/>.
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
					case 3:
						return W;
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
					case 3:
						W = value;
						return;
					default:
						throw new ArgumentOutOfRangeException( nameof( index ) );
				}
			}
		}

		/// <summary>
		/// Helper method for deconstruction into a tuple.
		/// </summary>
		public readonly void Deconstruct( out double x, out double y, out double z, out double w )
		{
			x = X;
			y = Y;
			z = Z;
			w = W;
		}

		internal void Normalize()
		{
			double lengthsq = LengthSquared();

			if ( lengthsq == 0 )
			{
				X = Y = Z = W = 0f;
			}
			else
			{
				double length = Math.Sqrt( lengthsq );
				X /= length;
				Y /= length;
				Z /= length;
				W /= length;
			}
		}

		/// <summary>
		/// Returns a new vector with all components in absolute values (i.e. positive).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Abs(double)"/> called on each component.</returns>
		public readonly Vector4D Abs()
		{
			return new Vector4D( Math.Abs( X ), Math.Abs( Y ), Math.Abs( Z ), Math.Abs( W ) );
		}

		/// <summary>
		/// Returns a new vector with all components rounded up (towards positive infinity).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Ceiling(double)"/> called on each component.</returns>
		public readonly Vector4D Ceiling()
		{
			return new Vector4D( Math.Ceiling( X ), Math.Ceiling( Y ), Math.Ceiling( Z ), Math.Ceiling( W ) );
		}

		/// <summary>
		/// Returns a new vector with all components clamped between the
		/// components of <paramref name="min"/> and <paramref name="max"/> using
		/// <see cref="Math.Clamp(double, double, double)"/>.
		/// </summary>
		/// <param name="min">The vector with minimum allowed values.</param>
		/// <param name="max">The vector with maximum allowed values.</param>
		/// <returns>The vector with all components clamped.</returns>
		public readonly Vector4D Clamp( Vector4D min, Vector4D max )
		{
			return new Vector4D
			(
				Math.Clamp( X, min.X, max.X ),
				Math.Clamp( Y, min.Y, max.Y ),
				Math.Clamp( Z, min.Z, max.Z ),
				Math.Clamp( W, min.W, max.W )
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
		public readonly Vector4D CubicInterpolate( Vector4D b, Vector4D preA, Vector4D postB, double weight )
		{
			return new Vector4D
			(
				Utils.Cubic( X, b.X, preA.X, postB.X, weight ),
				Utils.Cubic( Y, b.Y, preA.Y, postB.Y, weight ),
				Utils.Cubic( Z, b.Z, preA.Z, postB.Z, weight ),
				Utils.Cubic( W, b.W, preA.W, postB.W, weight )
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
		public readonly Vector4D CubicInterpolateInTime( Vector4D b, Vector4D preA, Vector4D postB, double weight, double t, double preAT, double postBT )
		{
			return new Vector4D
			(
				Utils.CubicInTime( X, b.X, preA.X, postB.X, weight, t, preAT, postBT ),
				Utils.CubicInTime( Y, b.Y, preA.Y, postB.Y, weight, t, preAT, postBT ),
				Utils.CubicInTime( Z, b.Z, preA.Z, postB.Z, weight, t, preAT, postBT ),
				Utils.CubicInTime( W, b.W, preA.W, postB.W, weight, t, preAT, postBT )
			);
		}

		/// <summary>
		/// Returns the normalized vector pointing from this vector to <paramref name="to"/>.
		/// </summary>
		/// <param name="to">The other vector to point towards.</param>
		/// <returns>The direction from this vector to <paramref name="to"/>.</returns>
		public readonly Vector4D DirectionTo( Vector4D to )
		{
			Vector4D ret = new Vector4D( to.X - X, to.Y - Y, to.Z - Z, to.W - W );
			ret.Normalize();
			return ret;
		}

		/// <summary>
		/// Returns the squared distance between this vector and <paramref name="to"/>.
		/// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
		/// you need to compare vectors or need the squared distance for some formula.
		/// </summary>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		public readonly double DistanceSquaredTo( Vector4D to )
		{
			return (to - this).LengthSquared();
		}

		/// <summary>
		/// Returns the distance between this vector and <paramref name="to"/>.
		/// </summary>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The distance between the two vectors.</returns>
		public readonly double DistanceTo( Vector4D to )
		{
			return (to - this).Length();
		}

		/// <summary>
		/// Returns the dot product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other vector to use.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public readonly double Dot( Vector4D with )
		{
			return (X * with.X) + (Y * with.Y) + (Z * with.Z) + (W * with.W);
		}

		/// <summary>
		/// Returns a new vector with all components rounded down (towards negative infinity).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Floor(double)"/> called on each component.</returns>
		public readonly Vector4D Floor()
		{
			return new Vector4D( Math.Floor( X ), Math.Floor( Y ), Math.Floor( Z ), Math.Floor( W ) );
		}

		/// <summary>
		/// Returns the inverse of this vector. This is the same as <c>new Vector4D(1 / v.X, 1 / v.Y, 1 / v.Z, 1 / v.W)</c>.
		/// </summary>
		/// <returns>The inverse of this vector.</returns>
		public readonly Vector4D Inverse()
		{
			return new Vector4D( 1 / X, 1 / Y, 1 / Z, 1 / W );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector is finite, by calling
		/// <see cref="double.IsFinite(double)"/> on each component.
		/// </summary>
		/// <returns>Whether this vector is finite or not.</returns>
		public readonly bool IsFinite()
		{
			return double.IsFinite( X ) && double.IsFinite( Y ) && double.IsFinite( Z ) && double.IsFinite( W );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vector is normalized, and <see langword="false"/> otherwise.
		/// </summary>
		/// <returns>A <see langword="bool"/> indicating whether or not the vector is normalized.</returns>
		public readonly bool IsNormalized()
		{
			return Math.Abs( LengthSquared() - 1.0f ) < double.Epsilon;
		}

		/// <summary>
		/// Returns the length (magnitude) of this vector.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		/// <returns>The length of this vector.</returns>
		public readonly double Length()
		{
			double x2 = X * X;
			double y2 = Y * Y;
			double z2 = Z * Z;
			double w2 = W * W;

			return Math.Sqrt( x2 + y2 + z2 + w2 );
		}

		/// <summary>
		/// Returns the squared length (squared magnitude) of this vector.
		/// This method runs faster than <see cref="Length"/>, so prefer it if
		/// you need to compare vectors or need the squared length for some formula.
		/// </summary>
		/// <returns>The squared length of this vector.</returns>
		public readonly double LengthSquared()
		{
			double x2 = X * X;
			double y2 = Y * Y;
			double z2 = Z * Z;
			double w2 = W * W;

			return x2 + y2 + z2 + w2;
		}

		/// <summary>
		/// Returns the result of the linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		/// </summary>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		public readonly Vector4D Lerp( Vector4D to, double weight )
		{
			return new Vector4D
			(
				Utils.Lerp( X, to.X, weight ),
				Utils.Lerp( Y, to.Y, weight ),
				Utils.Lerp( Z, to.Z, weight ),
				Utils.Lerp( W, to.W, weight )
			);
		}

		/// <summary>
		/// Returns the axis of the vector's highest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.X"/>.
		/// </summary>
		/// <returns>The index of the highest axis.</returns>
		public readonly Axis MaxAxisIndex()
		{
			int max_index = 0;
			double max_value = X;
			for ( int i = 1; i < 4; i++ )
			{
				if ( this[i] > max_value )
				{
					max_index = i;
					max_value = this[i];
				}
			}
			return (Axis)max_index;
		}

		/// <summary>
		/// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.W"/>.
		/// </summary>
		/// <returns>The index of the lowest axis.</returns>
		public readonly Axis MinAxisIndex()
		{
			int min_index = 0;
			double min_value = X;
			for ( int i = 1; i < 4; i++ )
			{
				if ( this[i] <= min_value )
				{
					min_index = i;
					min_value = this[i];
				}
			}
			return (Axis)min_index;
		}

		/// <summary>
		/// Returns the vector scaled to unit length. Equivalent to <c>v / v.Length()</c>.
		/// </summary>
		/// <returns>A normalized version of the vector.</returns>
		public readonly Vector4D Normalized()
		{
			Vector4D v = this;
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Returns this vector with all components rounded to the nearest integer,
		/// with halfway cases rounded towards the nearest multiple of two.
		/// </summary>
		/// <returns>The rounded vector.</returns>
		public readonly Vector4D Round()
		{
			return new Vector4D( Math.Round( X ), Math.Round( Y ), Math.Round( Z ), Math.Round( W ) );
		}

		/// <summary>
		/// Returns a vector with each component set to one or negative one, depending
		/// on the signs of this vector's components, or zero if the component is zero,
		/// by calling <see cref="Math.Sign(double)"/> on each component.
		/// </summary>
		/// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
		public readonly Vector4D Sign()
		{
			Vector4D v;
			v.X = Math.Sign( X );
			v.Y = Math.Sign( Y );
			v.Z = Math.Sign( Z );
			v.W = Math.Sign( W );
			return v;
		}

		/// <summary>
		/// Returns this vector with each component snapped to the nearest multiple of <paramref name="step"/>.
		/// This can also be used to round to an arbitrary number of decimals.
		/// </summary>
		/// <param name="step">A vector value representing the step size to snap to.</param>
		/// <returns>The snapped vector.</returns>
		public readonly Vector4D Snapped( Vector4D step )
		{
			return new Vector4D(
				Utils.Snapped( X, step.X ),
				Utils.Snapped( Y, step.Y ),
				Utils.Snapped( Z, step.Z ),
				Utils.Snapped( W, step.W )
			);
		}

		/// <summary>
		/// Zero vector, a vector with all components set to <c>0</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4D(0, 0, 0, 0)</c>.</value>
		public static Vector4D Zero => new( 0.0, 0.0, 0.0, 0.0 );
		/// <summary>
		/// One vector, a vector with all components set to <c>1</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4D(1, 1, 1, 1)</c>.</value>
		public static Vector4D One => new( 1.0, 1.0, 1.0, 1.0 );
		/// <summary>
		/// Infinity vector, a vector with all components set to <see cref="double.PositiveInfinity"/>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4D(Math.Inf, Math.Inf, Math.Inf, Math.Inf)</c>.</value>
		public static Vector4D Inf => new( double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity );

		/// <summary>
		/// X-facing unit vector. <c>(1 0 0 0)</c>
		/// </summary>
		public static Vector4D UnitX => new( 1.0, 0.0, 0.0, 0.0 );
		/// <summary>
		/// Y-facing unit vector. <c>(0 1 0 0)</c>
		/// </summary>
		public static Vector4D UnitY => new( 0.0, 1.0, 0.0, 0.0 );
		/// <summary>
		/// Z-facing unit vector. <c>(0 0 1 0)</c>
		/// </summary>
		public static Vector4D UnitZ => new( 0.0, 0.0, 1.0, 0.0 );
		/// <summary>
		/// W-facing unit vector. <c>(0 0 0 1)</c>
		/// </summary>
		public static Vector4D UnitW => new( 0.0, 0.0, 0.0, 1.0 );

		/// <summary>
		/// Constructs a new <see cref="Vector4D"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		/// <param name="z">The vector's Z component.</param>
		/// <param name="w">The vector's W component.</param>
		public Vector4D( float x, float y, float z, float w )
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Constructs a new <see cref="Vector4D"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		/// <param name="z">The vector's Z component.</param>
		/// <param name="w">The vector's W component.</param>
		public Vector4D( double x, double y, double z, double w )
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Adds each component of the <see cref="Vector4D"/>
		/// with the components of the given <see cref="Vector4D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The added vector.</returns>
		public static Vector4D operator +( Vector4D left, Vector4D right )
		{
			left.X += right.X;
			left.Y += right.Y;
			left.Z += right.Z;
			left.W += right.W;
			return left;
		}

		/// <summary>
		/// Subtracts each component of the <see cref="Vector4D"/>
		/// by the components of the given <see cref="Vector4D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The subtracted vector.</returns>
		public static Vector4D operator -( Vector4D left, Vector4D right )
		{
			left.X -= right.X;
			left.Y -= right.Y;
			left.Z -= right.Z;
			left.W -= right.W;
			return left;
		}

		/// <summary>
		/// Returns the negative value of the <see cref="Vector4D"/>.
		/// This is the same as writing <c>new Vector4D(-v.X, -v.Y, -v.Z, -v.W)</c>.
		/// This operation flips the direction of the vector while
		/// keeping the same magnitude.
		/// With floats, the number zero can be either positive or negative.
		/// </summary>
		/// <param name="vec">The vector to negate/flip.</param>
		/// <returns>The negated/flipped vector.</returns>
		public static Vector4D operator -( Vector4D vec )
		{
			vec.X = -vec.X;
			vec.Y = -vec.Y;
			vec.Z = -vec.Z;
			vec.W = -vec.W;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector4D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="vec">The vector to multiply.</param>
		/// <param name="scale">The scale to multiply by.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector4D operator *( Vector4D vec, double scale )
		{
			vec.X *= scale;
			vec.Y *= scale;
			vec.Z *= scale;
			vec.W *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector4D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="scale">The scale to multiply by.</param>
		/// <param name="vec">The vector to multiply.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector4D operator *( double scale, Vector4D vec )
		{
			vec.X *= scale;
			vec.Y *= scale;
			vec.Z *= scale;
			vec.W *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector4D"/>
		/// by the components of the given <see cref="Vector4D"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector4D operator *( Vector4D left, Vector4D right )
		{
			left.X *= right.X;
			left.Y *= right.Y;
			left.Z *= right.Z;
			left.W *= right.W;
			return left;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector4D"/>
		/// by the given <see cref="double"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The divided vector.</returns>
		public static Vector4D operator /( Vector4D vec, double divisor )
		{
			vec.X /= divisor;
			vec.Y /= divisor;
			vec.Z /= divisor;
			vec.W /= divisor;
			return vec;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector4D"/>
		/// by the components of the given <see cref="Vector4D"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The divided vector.</returns>
		public static Vector4D operator /( Vector4D vec, Vector4D divisorv )
		{
			vec.X /= divisorv.X;
			vec.Y /= divisorv.Y;
			vec.Z /= divisorv.Z;
			vec.W /= divisorv.W;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector4D"/>
		/// with the components of the given <see cref="double"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector4D(10, -20, 30, 40) % 7); // Prints "(3, -6, 2, 5)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector4D operator %( Vector4D vec, double divisor )
		{
			vec.X %= divisor;
			vec.Y %= divisor;
			vec.Z %= divisor;
			vec.W %= divisor;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector4D"/>
		/// with the components of the given <see cref="Vector4D"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector4D(10, -20, 30, 10) % new Vector4D(7, 8, 9, 10)); // Prints "(3, -4, 3, 0)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector4D operator %( Vector4D vec, Vector4D divisorv )
		{
			vec.X %= divisorv.X;
			vec.Y %= divisorv.Y;
			vec.Z %= divisorv.Z;
			vec.W %= divisorv.W;
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
		public static bool operator ==( Vector4D left, Vector4D right )
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
		public static bool operator !=( Vector4D left, Vector4D right )
		{
			return !left.Equals( right );
		}

		/// <summary>
		/// Compares two <see cref="Vector4D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than the right.</returns>
		public static bool operator <( Vector4D left, Vector4D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					if ( left.Z == right.Z )
					{
						return left.W < right.W;
					}
					return left.Z < right.Z;
				}
				return left.Y < right.Y;
			}
			return left.X < right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector4D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than the right.</returns>
		public static bool operator >( Vector4D left, Vector4D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					if ( left.Z == right.Z )
					{
						return left.W > right.W;
					}
					return left.Z > right.Z;
				}
				return left.Y > right.Y;
			}
			return left.X > right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector4D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than or equal to the right.</returns>
		public static bool operator <=( Vector4D left, Vector4D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					if ( left.Z == right.Z )
					{
						return left.W <= right.W;
					}
					return left.Z < right.Z;
				}
				return left.Y < right.Y;
			}
			return left.X < right.X;
		}

		/// <summary>
		/// Compares two <see cref="Vector4D"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than or equal to the right.</returns>
		public static bool operator >=( Vector4D left, Vector4D right )
		{
			if ( left.X == right.X )
			{
				if ( left.Y == right.Y )
				{
					if ( left.Z == right.Z )
					{
						return left.W >= right.W;
					}
					return left.Z > right.Z;
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
			return obj is Vector4D other && Equals( other );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are exactly equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="other">The other vector.</param>
		/// <returns>Whether or not the vectors are exactly equal.</returns>
		public readonly bool Equals( Vector4D other )
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Utils.IsEqualApprox(double, double)"/> on each component.
		/// </summary>
		/// <param name="other">The other vector to compare.</param>
		/// <returns>Whether or not the vectors are approximately equal.</returns>
		public readonly bool IsEqualApprox( Vector4D other )
		{
			return Utils.IsEqualApprox( X, other.X ) 
				&& Utils.IsEqualApprox( Y, other.Y ) 
				&& Utils.IsEqualApprox( Z, other.Z ) 
				&& Utils.IsEqualApprox( W, other.W );
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
			return Utils.IsZeroApprox( X ) 
				&& Utils.IsZeroApprox( Y ) 
				&& Utils.IsZeroApprox( Z ) 
				&& Utils.IsZeroApprox( W );
		}

		/// <summary>
		/// Serves as the hash function for <see cref="Vector4D"/>.
		/// </summary>
		/// <returns>A hash code for this vector.</returns>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine( X, Y, Z, W );
		}

		/// <summary>
		/// Converts this <see cref="Vector4D"/> to a string.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public override string ToString()
		{
			return $"({X}, {Y}, {Z}, {W})";
		}

		/// <summary>
		/// Converts this <see cref="Vector4D"/> to a string with the given <paramref name="format"/>.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public readonly string ToString( string? format )
		{
			return $"({X.ToString( format )}, {Y.ToString( format )}, {Z.ToString( format )}, {W.ToString( format )})";
		}
	}
}
