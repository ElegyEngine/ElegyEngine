// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Vector3.cs

using Elegy.Common.Maths;

namespace Elegy.Common.Extensions
{
	/// <summary>
	/// <see cref="Vector3"/> extensions from Godot's own Vector3 class. Lots of useful gamedev-specific stuff there.
	/// </summary>
	public static class Vector3GodotExtensions
	{
		/// <summary>
		/// Returns the Nth component of the vector.
		/// </summary>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public static float At( this Vector3 v, int i )
		{
			return i switch
			{
				0 => v.X,
				1 => v.Y,
				2 => v.Z,
				_ => throw new IndexOutOfRangeException()
			};
		}

		/// <summary>
		/// Returns a new vector with all components in absolute values (i.e. positive).
		/// </summary>
		/// <param name="v"></param>
		public static Vector3 Abs( this Vector3 v )
		{
			return Vector3.Abs( v );
		}

		/// <summary>
		/// Returns the unsigned minimum angle to the given vector, in radians.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <returns>The unsigned angle between the two vectors, in radians.</returns>
		public static float AngleTo( this Vector3 v, Vector3 to )
		{
			return MathF.Atan2(
				v.Cross( to ).Length(),
				v.Dot( to )
			);
		}

		/// <summary>
		/// Returns a new vector with all components rounded up (towards positive infinity).
		/// </summary>
		/// <param name="v"></param>
		public static Vector3 Ceil( this Vector3 v )
		{
			return new Vector3(
				MathF.Ceiling( v.X ),
				MathF.Ceiling( v.Y ),
				MathF.Ceiling( v.Z )
			);
		}

		/// <summary>
		/// Returns a new vector with all components clamped between the
		/// components of <paramref name="min"/> and <paramref name="max"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="min">The vector with minimum allowed values.</param>
		/// <param name="max">The vector with maximum allowed values.</param>
		public static Vector3 Clamp( this Vector3 v, Vector3 min, Vector3 max )
			=> Vector3.Clamp( v, min, max );


		/// <summary>
		/// Returns the cross product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="with">The other vector.</param>
		/// <returns>The cross product vector.</returns>
		public static Vector3 Cross( in this Vector3 v, in Vector3 with )
			=> Vector3.Cross( v, with );

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
		public static Vector3 CubicInterpolate( this Vector3 v, Vector3 b, Vector3 preA, Vector3 postB, float weight )
		{
			return new Vector3
			(
				Utils.Cubic( v.X, b.X, preA.X, postB.X, weight ),
				Utils.Cubic( v.Y, b.Y, preA.Y, postB.Y, weight ),
				Utils.Cubic( v.Z, b.Z, preA.Z, postB.Z, weight )
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
		public static Vector3 CubicInterpolateInTime( this Vector3 v, Vector3 b, Vector3 preA, Vector3 postB, float weight, float t, float preAT, float postBT )
		{
			return new Vector3
			(
				Utils.CubicInTime( v.X, b.X, preA.X, postB.X, weight, t, preAT, postBT ),
				Utils.CubicInTime( v.Y, b.Y, preA.Y, postB.Y, weight, t, preAT, postBT ),
				Utils.CubicInTime( v.Z, b.Z, preA.Z, postB.Z, weight, t, preAT, postBT )
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
		public static Vector3 BezierInterpolate( this Vector3 v, Vector3 control1, Vector3 control2, Vector3 end, float t )
		{
			return new Vector3
			(
				Utils.Bezier( v.X, control1.X, control2.X, end.X, t ),
				Utils.Bezier( v.Y, control1.Y, control2.Y, end.Y, t ),
				Utils.Bezier( v.Z, control1.Z, control2.Z, end.Z, t )
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
		public static Vector3 BezierDerivative( this Vector3 v, Vector3 control1, Vector3 control2, Vector3 end, float t )
		{
			return new Vector3(
				Utils.BezierDerivative( v.X, control1.X, control2.X, end.X, t ),
				Utils.BezierDerivative( v.Y, control1.Y, control2.Y, end.Y, t ),
				Utils.BezierDerivative( v.Z, control1.Z, control2.Z, end.Z, t )
			);
		}

		/// <summary>
		/// Returns the normalized vector pointing from this vector to <paramref name="to"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to point towards.</param>
		/// <returns>The direction from this vector to <paramref name="to"/>.</returns>
		public static Vector3 DirectionTo( this Vector3 v, Vector3 to )
		{
			return new Vector3( to.X - v.X, to.Y - v.Y, to.Z - v.Z ).Normalized();
		}

		/// <summary>
		/// Returns the squared distance between this vector and <paramref name="to"/>.
		/// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
		/// you need to compare vectors or need the squared distance for some formula.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		public static float DistanceSquaredTo( this Vector3 v, Vector3 to )
		{
			return (to - v).LengthSquared();
		}

		/// <summary>
		/// Returns the distance between this vector and <paramref name="to"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <seealso cref="DistanceSquaredTo"/>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The distance between the two vectors.</returns>
		public static float DistanceTo( this Vector3 v, Vector3 to )
		{
			return (to - v).Length();
		}

		/// <summary>
		/// Returns the dot product of this vector and <paramref name="with"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="with">The other vector to use.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static float Dot( in this Vector3 v, in Vector3 with )
			=> Vector3.Dot( v, with );

		/// <summary>
		/// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Utils.IsEqualApprox(float, float)"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="other">The other vector to compare.</param>
		/// <returns>Whether or not the vectors are approximately equal.</returns>
		public static bool IsEqualApprox( this Vector3 v, Vector3 other )
		{
			return Utils.IsEqualApprox( v.X, other.X )
				&& Utils.IsEqualApprox( v.Y, other.Y )
				&& Utils.IsEqualApprox( v.Z, other.Z );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Utils.IsEqualApprox(float, float, float)"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="other">The other vector to compare.</param>
		/// <param name="tolerance">The approximate radius of comparison.</param>
		/// <returns>Whether or not the vectors are approximately equal.</returns>
		public static bool IsEqualApprox( this Vector3 v, Vector3 other, float tolerance )
		{
			return Utils.IsEqualApprox( v.X, other.X, tolerance )
				&& Utils.IsEqualApprox( v.Y, other.Y, tolerance )
				&& Utils.IsEqualApprox( v.Z, other.Z, tolerance );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector's values are approximately zero,
		/// by running <see cref="Utils.IsZeroApprox(float)"/> on each component.
		/// This method is faster than using <see cref="IsEqualApprox"/> with one value
		/// as a zero vector.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Whether or not the vector is approximately zero.</returns>
		public static bool IsZeroApprox( this Vector3 v )
		{
			return Utils.IsZeroApprox( v.X )
				&& Utils.IsZeroApprox( v.Y )
				&& Utils.IsZeroApprox( v.Z );
		}

		/// <summary>
		/// Returns <see langword="true"/> if this vector is finite, by calling
		/// <see cref="float.IsFinite"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Whether this vector is finite or not.</returns>
		public static bool IsFinite( this Vector3 v )
		{
			return float.IsFinite( v.X ) && float.IsFinite( v.Y ) && float.IsFinite( v.Z );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vector is normalized, and <see langword="false"/> otherwise.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>A <see langword="bool"/> indicating whether or not the vector is normalized.</returns>
		public static bool IsNormalized( in this Vector3 v )
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
		public static Vector3 Lerp( in this Vector3 v, Vector3 to, float weight )
		{
			return new Vector3
			(
				Utils.Lerp( v.X, to.X, weight ),
				Utils.Lerp( v.Y, to.Y, weight ),
				Utils.Lerp( v.Z, to.Z, weight )
			);
		}

		/// <summary>
		/// Returns the vector with a maximum length by limiting its length to <paramref name="length"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="length">The length to limit to.</param>
		/// <returns>The vector with its length limited.</returns>
		public static Vector3 LimitLength( in this Vector3 v, float length = 1.0f )
		{
			Vector3 vcopy = v;
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
		public static Axis MaxAxisIndex( in this Vector3 v )
		{
			return v.X < v.Y ? (v.Y < v.Z ? Axis.Z : Axis.Y) : (v.X < v.Z ? Axis.Z : Axis.X);
		}

		/// <summary>
		/// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.Z"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The index of the lowest axis.</returns>
		public static Axis MinAxisIndex( in this Vector3 v )
		{
			return v.X < v.Y ? (v.X < v.Z ? Axis.X : Axis.Z) : (v.Y < v.Z ? Axis.Y : Axis.Z);
		}

		/// <summary>
		/// Moves this vector toward <paramref name="to"/> by the fixed <paramref name="delta"/> amount.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The vector to move towards.</param>
		/// <param name="delta">The amount to move towards by.</param>
		/// <returns>The resulting vector.</returns>
		public static Vector3 MoveToward( this Vector3 v, Vector3 to, float delta )
		{
			Vector3 v2 = v;
			Vector3 vd = to - v2;
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
		public static Vector3 Normalized( in this Vector3 v )
		{
			return Vector3.Normalize( v );
		}

		/// <summary>
		/// Returns this vector reflected from a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="normal">The normal vector defining the plane to reflect from. Must be normalized.</param>
		/// <returns>The reflected vector.</returns>
		public static Vector3 Reflect( this Vector3 v, in Vector3 normal )
			=> Vector3.Reflect( v, normal );

		/// <summary>
		/// Rotates this vector around a given <paramref name="axis"/> vector by <paramref name="angle"/> (in radians).
		/// The <paramref name="axis"/> vector must be a normalized vector.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="axis">The vector to rotate around. Must be normalized.</param>
		/// <param name="angle">The angle to rotate by, in radians.</param>
		/// <returns>The rotated vector.</returns>
		public static Vector3 Rotated( in this Vector3 v, Vector3 axis, float angle )
		{
#if DEBUG
			if ( !axis.IsNormalized() )
			{
				throw new ArgumentException( "Argument is not normalized.", nameof( axis ) );
			}
#endif
			return Vector3.Transform( v, Quaternion.CreateFromAxisAngle( axis, angle ) );
		}

		/// <summary>
		/// Returns this vector with all components rounded to the nearest integer,
		/// with halfway cases rounded towards the nearest multiple of two.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The rounded vector.</returns>
		public static Vector3 Round( this Vector3 v )
		{
			return new Vector3(
				MathF.Round( v.X ),
				MathF.Round( v.Y ),
				MathF.Round( v.Z )
			);
		}

		/// <summary>
		/// Returns a vector with each component set to one or negative one, depending
		/// on the signs of this vector's components, or zero if the component is zero,
		/// by calling <see cref="MathF.Sign(float)"/> on each component.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
		public static Vector3 Sign( this Vector3 v )
		{
			return new Vector3(
				MathF.Sign( v.X ),
				MathF.Sign( v.Y ),
				MathF.Sign( v.Z )
			);
		}

		/// <summary>
		/// Returns the signed angle to the given vector, in radians.
		/// The sign of the angle is positive in a counter-clockwise
		/// direction and negative in a clockwise direction when viewed
		/// from the side specified by the <paramref name="axis"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The other vector to compare this vector to.</param>
		/// <param name="axis">The reference axis to use for the angle sign.</param>
		/// <returns>The signed angle between the two vectors, in radians.</returns>
		public static float SignedAngleTo( this Vector3 v, Vector3 to, Vector3 axis )
		{
			Vector3 crossTo = v.Cross( to );
			float unsignedAngle = MathF.Atan2( crossTo.Length(), v.Dot( to ) );
			float sign = crossTo.Dot( axis );
			return (sign < 0.0f) ? -unsignedAngle : unsignedAngle;
		}

		/// <summary>
		/// Returns the result of the spherical linear interpolation between
		/// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
		///
		/// This method also handles interpolating the lengths if the input vectors
		/// have different lengths. For the special case of one or both input vectors
		/// having zero length, this method behaves like <see cref="Lerp(in Vector3, Vector3, float)"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="to">The destination vector for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting vector of the interpolation.</returns>
		public static Vector3 Slerp( in this Vector3 v, Vector3 to, float weight )
		{
			float startLengthSquared = v.LengthSquared();
			float endLengthSquared = to.LengthSquared();
			if ( startLengthSquared == 0.0 || endLengthSquared == 0.0 )
			{
				// Zero length vectors have no angle, so the best we can do is either lerp or throw an error.
				return v.Lerp( to, weight );
			}
			float startLength = MathF.Sqrt( startLengthSquared );
			float resultLength = Utils.Lerp( startLength, MathF.Sqrt( endLengthSquared ), weight );
			float angle = v.AngleTo( to );

			return v.Rotated( v.Cross( to ).Normalized(), angle * weight ) * (resultLength / startLength);
		}

		/// <summary>
		/// Returns this vector slid along a plane defined by the given <paramref name="normal"/>.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="normal">The normal vector defining the plane to slide on.</param>
		/// <returns>The slid vector.</returns>
		public static Vector3 Slide( in this Vector3 v, Vector3 normal )
		{
			return v - (normal * v.Dot( normal ));
		}

		/// <summary>
		/// Returns this vector with each component snapped to the nearest multiple of <paramref name="step"/>.
		/// This can also be used to round to an arbitrary number of decimals.
		/// </summary>
		/// <param name="v"></param>
		/// <param name="step">A vector value representing the step size to snap to.</param>
		/// <returns>The snapped vector.</returns>
		public static Vector3 Snapped( in this Vector3 v, Vector3 step )
		{
			return new Vector3
			(
				Utils.Snapped( v.X, step.X ),
				Utils.Snapped( v.Y, step.Y ),
				Utils.Snapped( v.Z, step.Z )
			);
		}
	}
}
