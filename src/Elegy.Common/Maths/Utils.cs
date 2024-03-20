// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Mathf.cs

using System.Runtime.CompilerServices;

namespace Elegy.Common.Maths
{
	public static class Utils
	{
		#region Linear
		/// <summary>
		/// Returns a normalized value considering the given range.
		/// This is the opposite of <see cref="Lerp(float, float, float)"/>.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="weight">The interpolated value.</param>
		/// <returns>
		/// The resulting value of the inverse interpolation.
		/// The returned value will be between 0.0 and 1.0 if <paramref name="weight"/> is
		/// between <paramref name="from"/> and <paramref name="to"/> (inclusive).
		/// </returns>
		public static float InverseLerp( float from, float to, float weight )
		{
			return (weight - from) / (to - from);
		}

		/// <summary>
		/// Returns a normalized value considering the given range.
		/// This is the opposite of <see cref="Lerp(double, double, double)"/>.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="weight">The interpolated value.</param>
		/// <returns>
		/// The resulting value of the inverse interpolation.
		/// The returned value will be between 0.0 and 1.0 if <paramref name="weight"/> is
		/// between <paramref name="from"/> and <paramref name="to"/> (inclusive).
		/// </returns>
		public static double InverseLerp( double from, double to, double weight )
		{
			return (weight - from) / (to - from);
		}

		/// <summary>
		/// Linearly interpolates between two values by a normalized value.
		/// This is the opposite <see cref="InverseLerp(float, float, float)"/>.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float Lerp( float from, float to, float weight )
		{
			return from + ((to - from) * weight);
		}

		/// <summary>
		/// Linearly interpolates between two values by a normalized value.
		/// This is the opposite <see cref="InverseLerp(double, double, double)"/>.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double Lerp( double from, double to, double weight )
		{
			return from + ((to - from) * weight);
		}

		/// <summary>
		/// Linearly interpolates between two angles (in radians) by a normalized value.
		///
		/// Similar to <see cref="Lerp(float, float, float)"/>,
		/// but interpolates correctly when the angles wrap around <see cref="Math.Tau"/>.
		/// </summary>
		/// <param name="from">The start angle for interpolation.</param>
		/// <param name="to">The destination angle for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting angle of the interpolation.</returns>
		public static float LerpAngle( float from, float to, float weight )
		{
			return from + AngleDifference( from, to ) * weight;
		}

		/// <summary>
		/// Linearly interpolates between two angles (in radians) by a normalized value.
		///
		/// Similar to <see cref="Lerp(double, double, double)"/>,
		/// but interpolates correctly when the angles wrap around <see cref="Math.Tau"/>.
		/// </summary>
		/// <param name="from">The start angle for interpolation.</param>
		/// <param name="to">The destination angle for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting angle of the interpolation.</returns>
		public static double LerpAngle( double from, double to, double weight )
		{
			return from + AngleDifference( from, to ) * weight;
		}

		/// <summary>
		/// Moves <paramref name="from"/> toward <paramref name="to"/> by the <paramref name="delta"/> value.
		///
		/// Use a negative <paramref name="delta"/> value to move away.
		/// </summary>
		/// <param name="from">The start value.</param>
		/// <param name="to">The value to move towards.</param>
		/// <param name="delta">The amount to move by.</param>
		/// <returns>The value after moving.</returns>
		public static float MoveToward( float from, float to, float delta )
		{
			if ( Math.Abs( to - from ) <= delta )
				return to;

			return from + (Math.Sign( to - from ) * delta);
		}

		/// <summary>
		/// Moves <paramref name="from"/> toward <paramref name="to"/> by the <paramref name="delta"/> value.
		///
		/// Use a negative <paramref name="delta"/> value to move away.
		/// </summary>
		/// <param name="from">The start value.</param>
		/// <param name="to">The value to move towards.</param>
		/// <param name="delta">The amount to move by.</param>
		/// <returns>The value after moving.</returns>
		public static double MoveToward( double from, double to, double delta )
		{
			if ( Math.Abs( to - from ) <= delta )
				return to;

			return from + (Math.Sign( to - from ) * delta);
		}
		#endregion

		#region Angular
		/// <summary>
		/// Returns the difference between the two angles,
		/// in range of -<see cref="MathF.PI"/>, <see cref="MathF.PI"/>.
		/// When <paramref name="from"/> and <paramref name="to"/> are opposite,
		/// returns -<see cref="MathF.PI"/> if <paramref name="from"/> is smaller than <paramref name="to"/>,
		/// or <see cref="MathF.PI"/> otherwise.
		/// </summary>
		/// <param name="from">The start angle.</param>
		/// <param name="to">The destination angle.</param>
		/// <returns>The difference between the two angles.</returns>
		public static float AngleDifference( float from, float to )
		{
			float difference = (to - from) % MathF.Tau;
			return ((2.0f * difference) % MathF.Tau) - difference;
		}

		/// <summary>
		/// Returns the difference between the two angles,
		/// in range of -<see cref="MathF.PI"/>, <see cref="MathF.PI"/>.
		/// When <paramref name="from"/> and <paramref name="to"/> are opposite,
		/// returns -<see cref="MathF.PI"/> if <paramref name="from"/> is smaller than <paramref name="to"/>,
		/// or <see cref="MathF.PI"/> otherwise.
		/// </summary>
		/// <param name="from">The start angle.</param>
		/// <param name="to">The destination angle.</param>
		/// <returns>The difference between the two angles.</returns>
		public static double AngleDifference( double from, double to )
		{
			double difference = (to - from) % Math.Tau;
			return ((2.0 * difference) % Math.Tau) - difference;
		}

		/// <summary>
		/// Rotates <paramref name="from"/> toward <paramref name="to"/> by the <paramref name="delta"/> amount. Will not go past <paramref name="to"/>.
		/// Similar to <see cref="MoveToward(float, float, float)"/> but interpolates correctly when the angles wrap around <see cref="MathF.Tau"/>.
		/// If <paramref name="delta"/> is negative, this function will rotate away from <paramref name="to"/>, toward the opposite angle, and will not go past the opposite angle.
		/// </summary>
		/// <param name="from">The start angle.</param>
		/// <param name="to">The angle to move towards.</param>
		/// <param name="delta">The amount to move by.</param>
		/// <returns>The angle after moving.</returns>
		public static float RotateToward( float from, float to, float delta )
		{
			float difference = AngleDifference( from, to );
			float absDifference = Math.Abs( difference );
			return from + Math.Clamp( delta, absDifference - MathF.PI, absDifference ) * (difference >= 0.0f ? 1.0f : -1.0f);
		}

		/// <summary>
		/// Rotates <paramref name="from"/> toward <paramref name="to"/> by the <paramref name="delta"/> amount. Will not go past <paramref name="to"/>.
		/// Similar to <see cref="MoveToward(double, double, double)"/> but interpolates correctly when the angles wrap around <see cref="Math.Tau"/>.
		/// If <paramref name="delta"/> is negative, this function will rotate away from <paramref name="to"/>, toward the opposite angle, and will not go past the opposite angle.
		/// </summary>
		/// <param name="from">The start angle.</param>
		/// <param name="to">The angle to move towards.</param>
		/// <param name="delta">The amount to move by.</param>
		/// <returns>The angle after moving.</returns>
		public static double RotateToward( double from, double to, double delta )
		{
			double difference = AngleDifference( from, to );
			double absDifference = Math.Abs( difference );
			return from + Math.Clamp( delta, absDifference - Math.PI, absDifference ) * (difference >= 0.0 ? 1.0 : -1.0);
		}


		#endregion

		#region Cubic
		/// <summary>
		/// Cubic interpolates between two values by the factor defined in <paramref name="weight"/>
		/// with pre and post values.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float Cubic( float from, float to, float pre, float post, float weight )
		{
			return 0.5f *
					((from * 2.0f) +
							(-pre + to) * weight +
							(2.0f * pre - 5.0f * from + 4.0f * to - post) * (weight * weight) +
							(-pre + 3.0f * from - 3.0f * to + post) * (weight * weight * weight));
		}

		/// <summary>
		/// Cubic interpolates between two values by the factor defined in <paramref name="weight"/>
		/// with pre and post values.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double Cubic( double from, double to, double pre, double post, double weight )
		{
			return 0.5 *
					((from * 2.0) +
							(-pre + to) * weight +
							(2.0 * pre - 5.0 * from + 4.0 * to - post) * (weight * weight) +
							(-pre + 3.0 * from - 3.0 * to + post) * (weight * weight * weight));
		}

		/// <summary>
		/// Cubic interpolates between two rotation values with shortest path
		/// by the factor defined in <paramref name="weight"/> with pre and post values.
		/// See also <see cref="LerpAngle(float, float, float)"/>.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float CubicAngle( float from, float to, float pre, float post, float weight )
		{
			float fromRot = from % MathF.Tau;

			float preDiff = (pre - fromRot) % MathF.Tau;
			float preRot = fromRot + (2.0f * preDiff) % MathF.Tau - preDiff;

			float toDiff = (to - fromRot) % MathF.Tau;
			float toRot = fromRot + (2.0f * toDiff) % MathF.Tau - toDiff;

			float postDiff = (post - toRot) % MathF.Tau;
			float postRot = toRot + (2.0f * postDiff) % MathF.Tau - postDiff;

			return Cubic( fromRot, toRot, preRot, postRot, weight );
		}

		/// <summary>
		/// Cubic interpolates between two rotation values with shortest path
		/// by the factor defined in <paramref name="weight"/> with pre and post values.
		/// See also <see cref="LerpAngle(double, double, double)"/>.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double CubicAngle( double from, double to, double pre, double post, double weight )
		{
			double fromRot = from % Math.Tau;

			double preDiff = (pre - fromRot) % Math.Tau;
			double preRot = fromRot + (2.0 * preDiff) % Math.Tau - preDiff;

			double toDiff = (to - fromRot) % Math.Tau;
			double toRot = fromRot + (2.0 * toDiff) % Math.Tau - toDiff;

			double postDiff = (post - toRot) % Math.Tau;
			double postRot = toRot + (2.0 * postDiff) % Math.Tau - postDiff;

			return Cubic( fromRot, toRot, preRot, postRot, weight );
		}

		/// <summary>
		/// Cubic interpolates between two values by the factor defined in <paramref name="weight"/>
		/// with pre and post values.
		/// It can perform smoother interpolation than
		/// <see cref="Cubic(float, float, float, float, float)"/>
		/// by the time values.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <param name="toT"></param>
		/// <param name="preT"></param>
		/// <param name="postT"></param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float CubicInTime( float from, float to, float pre, float post, float weight, float toT, float preT, float postT )
		{
			/* Barry-Goldman method */
			float t = Lerp( 0.0f, toT, weight );
			float a1 = Lerp( pre, from, preT == 0 ? 0.0f : (t - preT) / -preT );
			float a2 = Lerp( from, to, toT == 0 ? 0.5f : t / toT );
			float a3 = Lerp( to, post, postT - toT == 0 ? 1.0f : (t - toT) / (postT - toT) );
			float b1 = Lerp( a1, a2, toT - preT == 0 ? 0.0f : (t - preT) / (toT - preT) );
			float b2 = Lerp( a2, a3, postT == 0 ? 1.0f : t / postT );
			return Lerp( b1, b2, toT == 0 ? 0.5f : t / toT );
		}

		/// <summary>
		/// Cubic interpolates between two values by the factor defined in <paramref name="weight"/>
		/// with pre and post values.
		/// It can perform smoother interpolation than
		/// <see cref="Cubic(double, double, double, double, double)"/>
		/// by the time values.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <param name="toT"></param>
		/// <param name="preT"></param>
		/// <param name="postT"></param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double CubicInTime( double from, double to, double pre, double post, double weight, double toT, double preT, double postT )
		{
			/* Barry-Goldman method */
			double t = Lerp( 0.0, toT, weight );
			double a1 = Lerp( pre, from, preT == 0 ? 0.0 : (t - preT) / -preT );
			double a2 = Lerp( from, to, toT == 0 ? 0.5 : t / toT );
			double a3 = Lerp( to, post, postT - toT == 0 ? 1.0 : (t - toT) / (postT - toT) );
			double b1 = Lerp( a1, a2, toT - preT == 0 ? 0.0 : (t - preT) / (toT - preT) );
			double b2 = Lerp( a2, a3, postT == 0 ? 1.0 : t / postT );
			return Lerp( b1, b2, toT == 0 ? 0.5 : t / toT );
		}

		/// <summary>
		/// Cubic interpolates between two rotation values with shortest path
		/// by the factor defined in <paramref name="weight"/> with pre and post values.
		/// See also <see cref="LerpAngle(float, float, float)"/>.
		/// It can perform smoother interpolation than
		/// <see cref="CubicAngle(float, float, float, float, float)"/>
		/// by the time values.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <param name="toT"></param>
		/// <param name="preT"></param>
		/// <param name="postT"></param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float CubicAngleInTime( float from, float to, float pre, float post, float weight, float toT, float preT, float postT )
		{
			float fromRot = from % MathF.Tau;

			float preDiff = (pre - fromRot) % MathF.Tau;
			float preRot = fromRot + (2.0f * preDiff) % MathF.Tau - preDiff;

			float toDiff = (to - fromRot) % MathF.Tau;
			float toRot = fromRot + (2.0f * toDiff) % MathF.Tau - toDiff;

			float postDiff = (post - toRot) % MathF.Tau;
			float postRot = toRot + (2.0f * postDiff) % MathF.Tau - postDiff;

			return CubicInTime( fromRot, toRot, preRot, postRot, weight, toT, preT, postT );
		}

		/// <summary>
		/// Cubic interpolates between two rotation values with shortest path
		/// by the factor defined in <paramref name="weight"/> with pre and post values.
		/// See also <see cref="LerpAngle(double, double, double)"/>.
		/// It can perform smoother interpolation than
		/// <see cref="CubicAngle(double, double, double, double, double)"/>
		/// by the time values.
		/// </summary>
		/// <param name="from">The start value for interpolation.</param>
		/// <param name="to">The destination value for interpolation.</param>
		/// <param name="pre">The value which before "from" value for interpolation.</param>
		/// <param name="post">The value which after "to" value for interpolation.</param>
		/// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <param name="toT"></param>
		/// <param name="preT"></param>
		/// <param name="postT"></param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double CubicAngleInTime( double from, double to, double pre, double post, double weight, double toT, double preT, double postT )
		{
			double fromRot = from % Math.Tau;

			double preDiff = (pre - fromRot) % Math.Tau;
			double preRot = fromRot + (2.0 * preDiff) % Math.Tau - preDiff;

			double toDiff = (to - fromRot) % Math.Tau;
			double toRot = fromRot + (2.0 * toDiff) % Math.Tau - toDiff;

			double postDiff = (post - toRot) % Math.Tau;
			double postRot = toRot + (2.0 * postDiff) % Math.Tau - postDiff;

			return CubicInTime( fromRot, toRot, preRot, postRot, weight, toT, preT, postT );
		}
		#endregion

		#region Bezier
		/// <summary>
		/// Returns the point at the given <paramref name="t"/> on a one-dimensional Bezier curve defined by
		/// the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="start">The start value for the interpolation.</param>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination value for the interpolation.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float Bezier( float start, float control1, float control2, float end, float t )
		{
			// Formula from Wikipedia article on Bezier curves
			float omt = 1.0f - t;
			float omt2 = omt * omt;
			float omt3 = omt2 * omt;
			float t2 = t * t;
			float t3 = t2 * t;

			return start * omt3 + control1 * omt2 * t * 3.0f + control2 * omt * t2 * 3.0f + end * t3;
		}

		/// <summary>
		/// Returns the point at the given <paramref name="t"/> on a one-dimensional Bezier curve defined by
		/// the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="start">The start value for the interpolation.</param>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination value for the interpolation.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double Bezier( double start, double control1, double control2, double end, double t )
		{
			// Formula from Wikipedia article on Bezier curves
			double omt = 1.0 - t;
			double omt2 = omt * omt;
			double omt3 = omt2 * omt;
			double t2 = t * t;
			double t3 = t2 * t;

			return start * omt3 + control1 * omt2 * t * 3.0 + control2 * omt * t2 * 3.0 + end * t3;
		}

		/// <summary>
		/// Returns the derivative at the given <paramref name="t"/> on a one dimensional Bezier curve defined by
		/// the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="start">The start value for the interpolation.</param>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination value for the interpolation.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static float BezierDerivative( float start, float control1, float control2, float end, float t )
		{
			// Formula from Wikipedia article on Bezier curves
			float omt = 1.0f - t;
			float omt2 = omt * omt;
			float t2 = t * t;

			float d = (control1 - start) * 3.0f * omt2 + (control2 - control1) * 6.0f * omt * t + (end - control2) * 3.0f * t2;
			return d;
		}

		/// <summary>
		/// Returns the derivative at the given <paramref name="t"/> on a one dimensional Bezier curve defined by
		/// the given <paramref name="control1"/>, <paramref name="control2"/>, and <paramref name="end"/> points.
		/// </summary>
		/// <param name="start">The start value for the interpolation.</param>
		/// <param name="control1">Control point that defines the bezier curve.</param>
		/// <param name="control2">Control point that defines the bezier curve.</param>
		/// <param name="end">The destination value for the interpolation.</param>
		/// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
		/// <returns>The resulting value of the interpolation.</returns>
		public static double BezierDerivative( double start, double control1, double control2, double end, double t )
		{
			// Formula from Wikipedia article on Bezier curves
			double omt = 1.0 - t;
			double omt2 = omt * omt;
			double t2 = t * t;

			double d = (control1 - start) * 3.0 * omt2 + (control2 - control1) * 6.0 * omt * t + (end - control2) * 3.0 * t2;
			return d;
		}
		#endregion

		#region Easing
		/// <summary>
		/// Easing function, based on exponent. The <paramref name="curve"/> values are:
		/// <c>0</c> is constant, <c>1</c> is linear, <c>0</c> to <c>1</c> is ease-in, <c>1</c> or more is ease-out.
		/// Negative values are in-out/out-in.
		/// </summary>
		/// <param name="s">The value to ease.</param>
		/// <param name="curve">
		/// <c>0</c> is constant, <c>1</c> is linear, <c>0</c> to <c>1</c> is ease-in, <c>1</c> or more is ease-out.
		/// </param>
		/// <returns>The eased value.</returns>
		public static float Ease( float s, float curve )
		{
			if ( s < 0.0f )
			{
				s = 0.0f;
			}
			else if ( s > 1.0f )
			{
				s = 1.0f;
			}

			if ( curve > 0.0f )
			{
				if ( curve < 1.0f )
				{
					return 1.0f - MathF.Pow( 1.0f - s, 1.0f / curve );
				}

				return MathF.Pow( s, curve );
			}

			if ( curve < 0.0f )
			{
				if ( s < 0.5f )
				{
					return MathF.Pow( s * 2.0f, -curve ) * 0.5f;
				}

				return ((1.0f - MathF.Pow( 1.0f - ((s - 0.5f) * 2.0f), -curve )) * 0.5f) + 0.5f;
			}

			return 0.0f;
		}

		/// <summary>
		/// Easing function, based on exponent. The <paramref name="curve"/> values are:
		/// <c>0</c> is constant, <c>1</c> is linear, <c>0</c> to <c>1</c> is ease-in, <c>1</c> or more is ease-out.
		/// Negative values are in-out/out-in.
		/// </summary>
		/// <param name="s">The value to ease.</param>
		/// <param name="curve">
		/// <c>0</c> is constant, <c>1</c> is linear, <c>0</c> to <c>1</c> is ease-in, <c>1</c> or more is ease-out.
		/// </param>
		/// <returns>The eased value.</returns>
		public static double Ease( double s, double curve )
		{
			if ( s < 0.0 )
			{
				s = 0.0;
			}
			else if ( s > 1.0 )
			{
				s = 1.0;
			}

			if ( curve > 0 )
			{
				if ( curve < 1.0 )
				{
					return 1.0 - Math.Pow( 1.0 - s, 1.0 / curve );
				}

				return Math.Pow( s, curve );
			}

			if ( curve < 0.0 )
			{
				if ( s < 0.5 )
				{
					return Math.Pow( s * 2.0, -curve ) * 0.5;
				}

				return ((1.0 - Math.Pow( 1.0 - ((s - 0.5) * 2.0), -curve )) * 0.5) + 0.5;
			}

			return 0.0;
		}
		#endregion

		#region Remapping
		/// <summary>
		/// Maps a <paramref name="value"/> from [<paramref name="inFrom"/>, <paramref name="inTo"/>]
		/// to [<paramref name="outFrom"/>, <paramref name="outTo"/>].
		/// </summary>
		/// <param name="value">The value to map.</param>
		/// <param name="inFrom">The start value for the input interpolation.</param>
		/// <param name="inTo">The destination value for the input interpolation.</param>
		/// <param name="outFrom">The start value for the output interpolation.</param>
		/// <param name="outTo">The destination value for the output interpolation.</param>
		/// <returns>The resulting mapped value mapped.</returns>
		public static float Remap( float value, float inFrom, float inTo, float outFrom, float outTo )
		{
			return Lerp( outFrom, outTo, InverseLerp( inFrom, inTo, value ) );
		}

		/// <summary>
		/// Maps a <paramref name="value"/> from [<paramref name="inFrom"/>, <paramref name="inTo"/>]
		/// to [<paramref name="outFrom"/>, <paramref name="outTo"/>].
		/// </summary>
		/// <param name="value">The value to map.</param>
		/// <param name="inFrom">The start value for the input interpolation.</param>
		/// <param name="inTo">The destination value for the input interpolation.</param>
		/// <param name="outFrom">The start value for the output interpolation.</param>
		/// <param name="outTo">The destination value for the output interpolation.</param>
		/// <returns>The resulting mapped value mapped.</returns>
		public static double Remap( double value, double inFrom, double inTo, double outFrom, double outTo )
		{
			return Lerp( outFrom, outTo, InverseLerp( inFrom, inTo, value ) );
		}
		#endregion

		#region Grid and snapping
		/// <summary>
		/// Returns the position of the first non-zero digit, after the
		/// decimal point. Note that the maximum return value is 10,
		/// which is a design decision in the implementation.
		/// </summary>
		/// <param name="step">The input value.</param>
		/// <returns>The position of the first non-zero digit.</returns>
		public static int StepDecimals( double step )
		{
			double[] sd = new double[]
			{
				0.9999,
				0.09999,
				0.009999,
				0.0009999,
				0.00009999,
				0.000009999,
				0.0000009999,
				0.00000009999,
				0.000000009999,
			};
			double abs = Math.Abs( step );
			double decs = abs - (int)abs; // Strip away integer part
			for ( int i = 0; i < sd.Length; i++ )
			{
				if ( decs >= sd[i] )
				{
					return i;
				}
			}
			return 0;
		}

		/// <summary>
		/// Snaps float value <paramref name="s"/> to a given <paramref name="step"/>.
		/// This can also be used to round a floating point number to an arbitrary number of decimals.
		/// </summary>
		/// <param name="s">The value to snap.</param>
		/// <param name="step">The step size to snap to.</param>
		/// <returns>The snapped value.</returns>
		public static float Snapped( float s, float step )
		{
			if ( step != 0.0f )
			{
				return MathF.Floor( (s / step) + 0.5f ) * step;
			}

			return s;
		}

		/// <summary>
		/// Snaps float value <paramref name="s"/> to a given <paramref name="step"/>.
		/// This can also be used to round a floating point number to an arbitrary number of decimals.
		/// </summary>
		/// <param name="s">The value to snap.</param>
		/// <param name="step">The step size to snap to.</param>
		/// <returns>The snapped value.</returns>
		public static double Snapped( double s, double step )
		{
			if ( step != 0.0f )
			{
				return Math.Floor( (s / step) + 0.5 ) * step;
			}

			return s;
		}
		#endregion

		#region Extremes
		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> are approximately equal
		/// to each other.
		/// The comparison is done using a tolerance calculation with <see cref="float.Epsilon"/>.
		/// </summary>
		/// <param name="a">One of the values.</param>
		/// <param name="b">The other value.</param>
		/// <returns>A <see langword="bool"/> for whether or not the two values are approximately equal.</returns>
		public static bool IsEqualApprox( float a, float b )
		{
			// Check for exact equality first, required to handle "infinity" values.
			if ( a == b )
			{
				return true;
			}
			// Then check for approximate equality.
			float tolerance = float.Epsilon * Math.Abs( a );
			if ( tolerance < float.Epsilon )
			{
				tolerance = float.Epsilon;
			}
			return Math.Abs( a - b ) < tolerance;
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> are approximately equal
		/// to each other.
		/// The comparison is done using a tolerance calculation with <see cref="double.Epsilon"/>.
		/// </summary>
		/// <param name="a">One of the values.</param>
		/// <param name="b">The other value.</param>
		/// <returns>A <see langword="bool"/> for whether or not the two values are approximately equal.</returns>
		public static bool IsEqualApprox( double a, double b )
		{
			// Check for exact equality first, required to handle "infinity" values.
			if ( a == b )
			{
				return true;
			}
			// Then check for approximate equality.
			double tolerance = double.Epsilon * Math.Abs( a );
			if ( tolerance < double.Epsilon )
			{
				tolerance = double.Epsilon;
			}
			return Math.Abs( a - b ) < tolerance;
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="s"/> is zero or almost zero.
		/// The comparison is done using a tolerance calculation with <see cref="float.Epsilon"/>.
		///
		/// This method is faster than using <see cref="IsEqualApprox(float, float)"/> with
		/// one value as zero.
		/// </summary>
		/// <param name="s">The value to check.</param>
		/// <returns>A <see langword="bool"/> for whether or not the value is nearly zero.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool IsZeroApprox( float s )
		{
			return Math.Abs( s ) < float.Epsilon;
		}

		/// <summary>
		/// Returns <see langword="true"/> if <paramref name="s"/> is zero or almost zero.
		/// The comparison is done using a tolerance calculation with <see cref="double.Epsilon"/>.
		///
		/// This method is faster than using <see cref="IsEqualApprox(double, double)"/> with
		/// one value as zero.
		/// </summary>
		/// <param name="s">The value to check.</param>
		/// <returns>A <see langword="bool"/> for whether or not the value is nearly zero.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool IsZeroApprox( double s )
		{
			return Math.Abs( s ) < double.Epsilon;
		}
		#endregion
	}
}
