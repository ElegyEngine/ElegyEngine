﻿// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: The contents of this file are adapted from Godot Engine source code:
// https://github.com/godotengine/godot/blob/master/modules/mono/glue/GodotSharp/GodotSharp/Core/Vector4.cs

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Elegy.Common.Maths
{
	/// <summary>
	/// 4-element structure that can be used to represent 4D grid coordinates or sets of integers.
	/// </summary>
	[Serializable]
	[StructLayout( LayoutKind.Sequential )]
	public struct Vector4SB : IEquatable<Vector4SB>
	{
		/// <summary>
		/// The vector's X component. Also accessible by using the index position <c>[0]</c>.
		/// </summary>
		public sbyte X;

		/// <summary>
		/// The vector's Y component. Also accessible by using the index position <c>[1]</c>.
		/// </summary>
		public sbyte Y;

		/// <summary>
		/// The vector's Z component. Also accessible by using the index position <c>[2]</c>.
		/// </summary>
		public sbyte Z;

		/// <summary>
		/// The vector's W component. Also accessible by using the index position <c>[3]</c>.
		/// </summary>
		public sbyte W;

		/// <summary>
		/// Access vector components using their <paramref name="index"/>.
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
		public sbyte this[int index]
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
		public readonly void Deconstruct( out int x, out int y, out int z, out int w )
		{
			x = X;
			y = Y;
			z = Z;
			w = W;
		}

		/// <summary>
		/// Returns a new vector with all components in absolute values (i.e. positive).
		/// </summary>
		/// <returns>A vector with <see cref="Math.Abs(int)"/> called on each component.</returns>
		public readonly Vector4SB Abs()
		{
			return new Vector4SB(
				(sbyte)Math.Abs( X ),
				(sbyte)Math.Abs( Y ),
				(sbyte)Math.Abs( Z ),
				(sbyte)Math.Abs( W )
			);
		}

		/// <summary>
		/// Returns a new vector with all components clamped between the
		/// components of <paramref name="min"/> and <paramref name="max"/> using
		/// <see cref="Math.Clamp(int, int, int)"/>.
		/// </summary>
		/// <param name="min">The vector with minimum allowed values.</param>
		/// <param name="max">The vector with maximum allowed values.</param>
		/// <returns>The vector with all components clamped.</returns>
		public readonly Vector4SB Clamp( Vector4SB min, Vector4SB max )
		{
			return new Vector4SB (
				Math.Clamp( X, min.X, max.X ),
				Math.Clamp( Y, min.Y, max.Y ),
				Math.Clamp( Z, min.Z, max.Z ),
				Math.Clamp( W, min.W, max.W )
			);
		}

		/// <summary>
		/// Returns the squared distance between this vector and <paramref name="to"/>.
		/// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
		/// you need to compare vectors or need the squared distance for some formula.
		/// </summary>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The squared distance between the two vectors.</returns>
		public readonly int DistanceSquaredTo( Vector4SB to )
		{
			return (to - this).LengthSquared();
		}

		/// <summary>
		/// Returns the distance between this vector and <paramref name="to"/>.
		/// </summary>
		/// <seealso cref="DistanceSquaredTo(Vector4SB)"/>
		/// <param name="to">The other vector to use.</param>
		/// <returns>The distance between the two vectors.</returns>
		public readonly float DistanceTo( Vector4SB to )
		{
			return (to - this).Length();
		}

		/// <summary>
		/// Returns the length (magnitude) of this vector.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		/// <returns>The length of this vector.</returns>
		public readonly float Length()
		{
			int x2 = X * X;
			int y2 = Y * Y;
			int z2 = Z * Z;
			int w2 = W * W;

			return MathF.Sqrt( x2 + y2 + z2 + w2 );
		}

		/// <summary>
		/// Returns the squared length (squared magnitude) of this vector.
		/// This method runs faster than <see cref="Length"/>, so prefer it if
		/// you need to compare vectors or need the squared length for some formula.
		/// </summary>
		/// <returns>The squared length of this vector.</returns>
		public readonly int LengthSquared()
		{
			int x2 = X * X;
			int y2 = Y * Y;
			int z2 = Z * Z;
			int w2 = W * W;

			return x2 + y2 + z2 + w2;
		}

		/// <summary>
		/// Returns the axis of the vector's highest value. See <see cref="Axis"/>.
		/// If all components are equal, this method returns <see cref="Axis.X"/>.
		/// </summary>
		/// <returns>The index of the highest axis.</returns>
		public readonly Axis MaxAxisIndex()
		{
			int max_index = 0;
			int max_value = X;
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
			int min_value = X;
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
		/// Returns a vector with each component set to one or negative one, depending
		/// on the signs of this vector's components, or zero if the component is zero,
		/// by calling <see cref="Math.Sign(int)"/> on each component.
		/// </summary>
		/// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
		public readonly Vector4SB Sign()
		{
			return new Vector4SB(
				(sbyte)Math.Sign( X ),
				(sbyte)Math.Sign( Y ),
				(sbyte)Math.Sign( Z ),
				(sbyte)Math.Sign( W )
			);
		}

		/// <summary>
		/// Min vector, a vector with all components equal to <see cref="int.MinValue"/>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4SB(sbyte.MinValue, sbyte.MinValue, sbyte.MinValue, sbyte.MinValue)</c>.</value>
		public static Vector4SB MinValue => new( sbyte.MinValue, sbyte.MinValue, sbyte.MinValue, sbyte.MinValue );
		/// <summary>
		/// Max vector, a vector with all components equal to <see cref="int.MaxValue"/>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4SB(sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue)</c>.</value>
		public static Vector4SB MaxValue => new( sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue );

		/// <summary>
		/// Zero vector, a vector with all components set to <c>0</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4SB(0, 0, 0, 0)</c>.</value>
		public static Vector4SB Zero => new( 0, 0, 0, 0 );
		/// <summary>
		/// One vector, a vector with all components set to <c>1</c>.
		/// </summary>
		/// <value>Equivalent to <c>new Vector4SB(1, 1, 1, 1)</c>.</value>
		public static Vector4SB One => new( 1, 1, 1, 1 );

		/// <summary>
		/// X-facing unit vector. <c>(1 0 0 0)</c>
		/// </summary>
		public static Vector4SB UnitX => new( 1, 0, 0, 0 );
		/// <summary>
		/// Y-facing unit vector. <c>(0 1 0 0)</c>
		/// </summary>
		public static Vector4SB UnitY => new( 0, 1, 0, 0 );
		/// <summary>
		/// Z-facing unit vector. <c>(0 0 1 0)</c>
		/// </summary>
		public static Vector4SB UnitZ => new( 0, 0, 1, 0 );
		/// <summary>
		/// W-facing unit vector. <c>(0 0 0 1)</c>
		/// </summary>
		public static Vector4SB UnitW => new( 0, 0, 0, 1 );

		/// <summary>
		/// Constructs a new <see cref="Vector4SB"/> with the given components.
		/// </summary>
		/// <param name="x">The vector's X component.</param>
		/// <param name="y">The vector's Y component.</param>
		/// <param name="z">The vector's Z component.</param>
		/// <param name="w">The vector's W component.</param>
		public Vector4SB( sbyte x, sbyte y, sbyte z, sbyte w )
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Adds each component of the <see cref="Vector4SB"/>
		/// with the components of the given <see cref="Vector4SB"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The added vector.</returns>
		public static Vector4SB operator +( Vector4SB left, Vector4SB right )
		{
			left.X += right.X;
			left.Y += right.Y;
			left.Z += right.Z;
			left.W += right.W;
			return left;
		}

		/// <summary>
		/// Subtracts each component of the <see cref="Vector4SB"/>
		/// by the components of the given <see cref="Vector4SB"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The subtracted vector.</returns>
		public static Vector4SB operator -( Vector4SB left, Vector4SB right )
		{
			left.X -= right.X;
			left.Y -= right.Y;
			left.Z -= right.Z;
			left.W -= right.W;
			return left;
		}

		/// <summary>
		/// Returns the negative value of the <see cref="Vector4SB"/>.
		/// This is the same as writing <c>new Vector4UB(-v.X, -v.Y, -v.Z, -v.W)</c>.
		/// This operation flips the direction of the vector while
		/// keeping the same magnitude.
		/// </summary>
		/// <param name="vec">The vector to negate/flip.</param>
		/// <returns>The negated/flipped vector.</returns>
		public static Vector4SB operator -( Vector4SB vec )
		{
			vec.X = (sbyte)-vec.X;
			vec.Y = (sbyte)-vec.Y;
			vec.Z = (sbyte)-vec.Z;
			vec.W = (sbyte)-vec.W;
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector4SB"/>
		/// by the given <see langword="int"/>.
		/// </summary>
		/// <param name="vec">The vector to multiply.</param>
		/// <param name="scale">The scale to multiply by.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector4SB operator *( Vector4SB vec, int scale )
		{
			vec.X = (sbyte)(vec.X * scale);
			vec.Y = (sbyte)(vec.Y * scale);
			vec.Z = (sbyte)(vec.Z * scale);
			vec.W = (sbyte)(vec.W * scale);
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector4SB"/>
		/// by the given <see langword="int"/>.
		/// </summary>
		/// <param name="scale">The scale to multiply by.</param>
		/// <param name="vec">The vector to multiply.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector4SB operator *( int scale, Vector4SB vec )
		{
			vec.X = (sbyte)(vec.X * scale);
			vec.Y = (sbyte)(vec.Y * scale);
			vec.Z = (sbyte)(vec.Z * scale);
			vec.W = (sbyte)(vec.W * scale);
			return vec;
		}

		/// <summary>
		/// Multiplies each component of the <see cref="Vector4SB"/>
		/// by the components of the given <see cref="Vector4SB"/>.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>The multiplied vector.</returns>
		public static Vector4SB operator *( Vector4SB left, Vector4SB right )
		{
			left.X *= right.X;
			left.Y *= right.Y;
			left.Z *= right.Z;
			left.W *= right.W;
			return left;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector4SB"/>
		/// by the given <see langword="int"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The divided vector.</returns>
		public static Vector4SB operator /( Vector4SB vec, int divisor )
		{
			vec.X = (sbyte)(vec.X / divisor);
			vec.Y = (sbyte)(vec.Y / divisor);
			vec.Z = (sbyte)(vec.Z / divisor);
			vec.W = (sbyte)(vec.W / divisor);
			return vec;
		}

		/// <summary>
		/// Divides each component of the <see cref="Vector4SB"/>
		/// by the components of the given <see cref="Vector4SB"/>.
		/// </summary>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The divided vector.</returns>
		public static Vector4SB operator /( Vector4SB vec, Vector4SB divisorv )
		{
			vec.X /= divisorv.X;
			vec.Y /= divisorv.Y;
			vec.Z /= divisorv.Z;
			vec.W /= divisorv.W;
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector4SB"/>
		/// with the components of the given <see langword="int"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector4UB(10, -20, 30, -40) % 7); // Prints "(3, -6, 2, -5)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisor">The divisor value.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector4SB operator %( Vector4SB vec, int divisor )
		{
			vec.X = (sbyte)(vec.X % divisor);
			vec.Y = (sbyte)(vec.Y % divisor);
			vec.Z = (sbyte)(vec.Z % divisor);
			vec.W = (sbyte)(vec.W % divisor);
			return vec;
		}

		/// <summary>
		/// Gets the remainder of each component of the <see cref="Vector4SB"/>
		/// with the components of the given <see cref="Vector4SB"/>.
		/// This operation uses truncated division, which is often not desired
		/// as it does not work well with negative numbers.
		/// </summary>
		/// <example>
		/// <code>
		/// GD.Print(new Vector4UB(10, -20, 30, -40) % new Vector4UB(6, 7, 8, 9)); // Prints "(4, -6, 6, -4)"
		/// </code>
		/// </example>
		/// <param name="vec">The dividend vector.</param>
		/// <param name="divisorv">The divisor vector.</param>
		/// <returns>The remainder vector.</returns>
		public static Vector4SB operator %( Vector4SB vec, Vector4SB divisorv )
		{
			vec.X %= divisorv.X;
			vec.Y %= divisorv.Y;
			vec.Z %= divisorv.Z;
			vec.W %= divisorv.W;
			return vec;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are equal.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the vectors are equal.</returns>
		public static bool operator ==( Vector4SB left, Vector4SB right )
		{
			return left.Equals( right );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are not equal.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the vectors are not equal.</returns>
		public static bool operator !=( Vector4SB left, Vector4SB right )
		{
			return !left.Equals( right );
		}

		/// <summary>
		/// Compares two <see cref="Vector4SB"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than the right.</returns>
		public static bool operator <( Vector4SB left, Vector4SB right )
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
		/// Compares two <see cref="Vector4SB"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than the right.</returns>
		public static bool operator >( Vector4SB left, Vector4SB right )
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
		/// Compares two <see cref="Vector4SB"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is less than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is less than or equal to the right.</returns>
		public static bool operator <=( Vector4SB left, Vector4SB right )
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
		/// Compares two <see cref="Vector4SB"/> vectors by first checking if
		/// the X value of the <paramref name="left"/> vector is greater than
		/// or equal to the X value of the <paramref name="right"/> vector.
		/// If the X values are exactly equal, then it repeats this check
		/// with the Y, Z and finally W values of the two vectors.
		/// This operator is useful for sorting vectors.
		/// </summary>
		/// <param name="left">The left vector.</param>
		/// <param name="right">The right vector.</param>
		/// <returns>Whether or not the left is greater than or equal to the right.</returns>
		public static bool operator >=( Vector4SB left, Vector4SB right )
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
		/// Converts this <see cref="Vector4SB"/> to a <see cref="Vector4"/>.
		/// </summary>
		/// <param name="value">The vector to convert.</param>
		public static implicit operator Vector4( Vector4SB value )
		{
			return new Vector4( value.X, value.Y, value.Z, value.W );
		}

		// TODO: If the XML warning brought you here about Vector4.Floor, Ceiling etc.
		// That means these aren't implemented as extensions yet. :)
		// We need a Vector4GodotExtensions, well, not really. But if you want completeness, go for it.

		/// <summary>
		/// Converts a <see cref="Vector4"/> to a <see cref="Vector4SB"/> by truncating
		/// components' fractional parts (rounding towards zero).
		/// </summary>
		/// <param name="value">The vector to convert.</param>
		public static explicit operator Vector4SB( Vector4 value )
		{
			return new Vector4SB( (sbyte)value.X, (sbyte)value.Y, (sbyte)value.Z, (sbyte)value.W );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vector is equal
		/// to the given object (<paramref name="obj"/>).
		/// </summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Whether or not the vector and the object are equal.</returns>
		public override readonly bool Equals( [NotNullWhen( true )] object? obj )
		{
			return obj is Vector4SB other && Equals( other );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the vectors are equal.
		/// </summary>
		/// <param name="other">The other vector.</param>
		/// <returns>Whether or not the vectors are equal.</returns>
		public readonly bool Equals( Vector4SB other )
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}

		/// <summary>
		/// Serves as the hash function for <see cref="Vector4SB"/>.
		/// </summary>
		/// <returns>A hash code for this vector.</returns>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine( X, Y, Z, W );
		}

		/// <summary>
		/// Converts this <see cref="Vector4SB"/> to a string.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public override readonly string ToString()
		{
			return $"({X}, {Y}, {Z}, {W})";
		}

		/// <summary>
		/// Converts this <see cref="Vector4SB"/> to a string with the given <paramref name="format"/>.
		/// </summary>
		/// <returns>A string representation of this vector.</returns>
		public readonly string ToString( string? format )
		{
			return $"({X.ToString( format )}, {Y.ToString( format )}, {Z.ToString( format )}), {W.ToString( format )})";
		}
	}
}
