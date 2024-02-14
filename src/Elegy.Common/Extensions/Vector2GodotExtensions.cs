// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Vector2.cs

using Elegy.Maths;

namespace Elegy.Extensions
{
	/// <summary>
	/// <see cref="Vector2"/> extensions from Godot's own Vector2 class. Lots of useful gamedev-specific stuff there.
	/// </summary>
	public static class Vector2GodotExtensions
	{
		/// <summary>
		/// Returns the Nth component of the vector.
		/// </summary>
		/// <param name="v"></param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public static float At( this Vector2 v, int i )
		{
			return i switch
			{
				0 => v.X,
				1 => v.Y,
				_ => throw new IndexOutOfRangeException()
			};
		}

		/// <summary>
		/// Returns a new vector with all components in absolute values (i.e. positive).
		/// </summary>
		/// <param name="v"></param>
		public static Vector2 Abs( this Vector2 v )
		{
			return Vector2.Abs( v );
		}

		/// <summary>
		/// Returns this vector's angle with respect to the X axis, or (1, 0) vector, in radians.
		///
		/// Equivalent to the result of <see cref="MathF.Atan2(float, float)"/> when
		/// called with the vector's <see cref="Vector2.Y"/> and <see cref="Vector2.X"/> as parameters: <c>Mathf.Atan2(v.Y, v.X)</c>.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The angle of this vector, in radians.</returns>
		public static float Angle( this Vector2 v )
		{
			return MathF.Atan2( v.Y, v.X );
		}

		/// <summary>
		/// Returns the angle to the given vector, in radians.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <returns>The angle between the two vectors, in radians.</returns>
		public static float AngleTo( this Vector2 v, Vector2 to )
		{
			return MathF.Atan2( v.Cross( to ), v.Dot( to ) );
		}

		/// <summary>
		/// Returns the angle between the line connecting the two points and the X axis, in radians.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <returns>The angle between the two vectors, in radians.</returns>
		public static float AngleToPoint( this Vector2 v, Vector2 to )
		{
			return MathF.Atan2( to.Y - v.Y, to.X - v.X );
		}

		/// <summary>
		/// Returns a new vector with all components rounded up (towards positive infinity).
		/// </summary>
		/// <param name="v"></param>
		public static Vector2 Ceil( this Vector2 v )
		{
			return new Vector2(
				MathF.Ceiling( v.X ),
				MathF.Ceiling( v.Y )
			);
		}

		/// <summary>
		/// Returns a new vector with all components clamped between the
		/// components of <paramref name="min"/> and <paramref name="max"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min">The vector with minimum allowed values.</param>
		/// <param name="max">The vector with maximum allowed values.</param>
		public static Vector2 Clamp( this Vector2 v, Vector2 min, Vector2 max )
			=> Vector2.Clamp( v, min, max );

		/// <summary>
		/// Returns the cross product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="with">The other vector.</param>
		/// <returns>The cross product value.</returns>
		public static float Cross( this Vector2 v, Vector2 with )
		{
			return (v.X * with.Y) - (v.Y * with.X);
		}

		/// <summary>
		/// Performs a cubic interpolation between vectors <paramref name="preA"/>, this vector,
		/// <paramref name="b"/>, and <paramref name="postB"/>, by the given amount <paramref name="weight"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="b">The destination vector.</param>
		/// <param name="preA">A vector before this vector.</param>
		/// <param name="postB">A vector after <paramref name="b"/>.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The interpolated vector.</returns>
		public static Vector2 CubicInterpolate( this Vector2 v, Vector2 b, Vector2 preA, Vector2 postB, float weight )
		{
			return new Vector2
			(
				Utils.Cubic( v.X, b.X, preA.X, postB.X, weight ),
				Utils.Cubic( v.Y, b.Y, preA.Y, postB.Y, weight )
			);
		}

		/// <summary>
		/// Performs a cubic interpolation between vectors <paramref name="preA"/>, this vector,
		/// <paramref name="b"/>, and <paramref name="postB"/>, by the given amount <paramref name="weight"/>.
		/// It can perform smoother interpolation than <see cref="CubicInterpolate"/>
		/// by the time values.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="b">The destination vector.</param>
		/// <param name="preA">A vector before this vector.</param>
		/// <param name="postB">A vector after <paramref name="b"/>.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <param name="t"></param>
		/// <param name="preAT"></param>
		/// <param name="postBT"></param>
		/// <returns>The interpolated vector.</returns>
		public static Vector2 CubicInterpolateInTime( this Vector2 v, Vector2 b, Vector2 preA, Vector2 postB, float weight, float t, float preAT, float postBT )
		{
			return new Vector2
			(
				Utils.CubicInTime( v.X, b.X, preA.X, postB.X, weight, t, preAT, postBT ),
				Utils.CubicInTime( v.Y, b.Y, preA.Y, postB.Y, weight, t, preAT, postBT )
			);
		}

		/// <summary>
		/// Returns the point at the given <paramref name="t"/> on a one-dimensional Bezier curve defined by this vector
		/// and the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination vector.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The interpolated vector.</returns>
		public static Vector2 BezierInterpolate( this Vector2 v, Vector2 control1, Vector2 control2, Vector2 end, float t )
		{
			return new Vector2
			(
				Utils.Bezier( v.X, control1.X, control2.X, end.X, t ),
				Utils.Bezier( v.Y, control1.Y, control2.Y, end.Y, t )
			);
		}

		/// <summary>
		/// Returns the derivative at the given <paramref name="t"/> on the Bezier curve defined by this vector
		/// and the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination value for the interpolation.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static Vector2 BezierDerivative( this Vector2 v, Vector2 control1, Vector2 control2, Vector2 end, float t )
		{
			return new Vector2(
				Utils.BezierDerivative( v.X, control1.X, control2.X, end.X, t ),
				Utils.BezierDerivative( v.Y, control1.Y, control2.Y, end.Y, t )
			);
		}

		/// <summary>
		/// Returns the normalized vector pointing from this vector to <paramref name="to"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to point towards.</param>
		/// <returns>The direction from this vector to <paramref name="to"/>.</returns>
		public static Vector2 DirectionTo( this Vector2 v, Vector2 to )
		{
			return new Vector2( to.X - v.X, to.Y - v.Y ).Normalize();
		}

		/// <summary>
		/// Returns the squared distance between this vector and <paramref name="to"/>.
		/// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
		/// you need to compare vectors or need the squared distance for some formula.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		public static float DistanceSquaredTo( this Vector2 v, Vector2 to )
		{
			return (to - v).LengthSquared();
		}

		/// <summary>
		/// Returns the distance between this vector and <paramref name="to"/>.
		/// </summary>
		/// <seealso cref="DistanceSquaredTo"/>
		/// <param name="v"></param>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The distance between the two vectors.</returns>
		public static float DistanceTo( this Vector2 v, Vector2 to )
		{
			return (to - v).Length();
		}

		/// <summary>
		/// Returns the dot product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="with">The other vector to use.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static float Dot( in this Vector2 v, in Vector2 with )
			=> Vector2.Dot( v, with );

		/// <summary>
		/// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Utils.IsEqualApprox(float, float)"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="other">The other vector to compare.</param>
		/// <returns>Whether or not the vectors are approximately equal.</returns>
		public static bool IsEqualApprox( this Vector2 v, Vector2 other )
		{
			return Utils.IsEqualApprox( v.X, other.X )
				&& Utils.IsEqualApprox( v.Y, other.Y );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector's values are approximately zero,
		/// by running <see cref="Utils.IsZeroApprox(float)"/> on each component.
		/// This method is faster than using <see cref="IsEqualApprox"/> with one value
		/// as a zero vector.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Whether or not the vector is approximately zero.</returns>
		public static bool IsZeroApprox( this Vector2 v )
		{
			return Utils.IsZeroApprox( v.X )
				&& Utils.IsZeroApprox( v.Y );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector is finite, by calling
		/// <see cref="float.IsFinite"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Whether this vector is finite or not.</returns>
		public static bool IsFinite( this Vector2 v )
		{
			return float.IsFinite( v.X ) && float.IsFinite( v.Y );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vector is normalized, and <see langword="false"/> otherwise.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>A <see langword="bool"/> indicating whether or not the vector is normalized.</returns>
		public static bool IsNormalized( in this Vector2 v )
		{
			return MathF.Abs( v.LengthSquared() - 1.0f ) < float.Epsilon;
		}

		/// <summary>
		/// Returns the result of the linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		public static Vector2 Lerp( in this Vector2 v, Vector2 to, float weight )
		{
			return new Vector2
			(
				Utils.Lerp( v.X, to.X, weight ),
				Utils.Lerp( v.Y, to.Y, weight )
			);
		}

		/// <summary>
		/// Returns the vector with a maximum length by limiting its length to <paramref name="length"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="length">The length to limit to.</param>
		/// <returns>The vector with its length limited.</returns>
		public static Vector2 LimitLength( in this Vector2 v, float length = 1.0f )
		{
			Vector2 vcopy = v;
			float l = v.Length();

			if ( l > 0 && length < l )
			{
				vcopy /= l;
				vcopy *= length;
			}

			return vcopy;
		}

		/// <summary>
		/// Returns the axis of the vector's highest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.X"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The index of the highest axis.</returns>
		public static Axis MaxAxisIndex( in this Vector2 v )
		{
			return v.X < v.Y ? Axis.Y : Axis.X;
		}

		/// <summary>
		/// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.Z"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The index of the lowest axis.</returns>
		public static Axis MinAxisIndex( in this Vector2 v )
		{
			return v.X < v.Y ? Axis.X : Axis.Y;
		}

		/// <summary>
		/// Moves this vector toward <paramref name="to"/> by the fixed <paramref name="delta"/> amount.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The vector to move towards.</param>
		/// <param name="delta">The amount to move towards by.</param>
		/// <returns>The resulting vector.</returns>
		public static Vector2 MoveToward( this Vector2 v, Vector2 to, float delta )
		{
			Vector2 v2 = v;
			Vector2 vd = to - v2;
			float len = vd.Length();
			if ( len <= delta || len < float.Epsilon )
				return to;

			return v2 + (vd / len * delta);
		}

		/// <summary>
		/// Returns the vector scaled to unit length. Equivalent to <c>v / v.Length()</c>.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>A normalized version of the vector.</returns>
		public static Vector2 Normalize( in this Vector2 v )
			=> Vector2.Normalize( v );

		/// <summary>
		/// Returns this vector reflected from a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="normal">The normal vector defining the plane to reflect from. Must be normalized.</param>
		/// <returns>The reflected vector.</returns>
		public static Vector2 Reflect( this Vector2 v, in Vector2 normal )
			=> Vector2.Reflect( v, normal );

		/// <summary>
		/// Returns this vector with all components rounded to the nearest integer,
		/// with halfway cases rounded towards the nearest multiple of two.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The rounded vector.</returns>
		public static Vector2 Round( this Vector2 v )
		{
			return new Vector2(
				MathF.Round( v.X ),
				MathF.Round( v.Y )
			);
		}

		/// <summary>
		/// Returns a vector with each component set to one or negative one, depending
		/// on the signs of this vector's components, or zero if the component is zero,
		/// by calling <see cref="MathF.Sign(float)"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
		public static Vector2 Sign( this Vector2 v )
		{
			return new Vector2(
				MathF.Sign( v.X ),
				MathF.Sign( v.Y )
			);
		}
	}
}
