// SPDX-FileCopyrightText: 2007-2014 Juan Linietsky, Ariel Manzur; 2014-present Godot Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Elegy.Common.Maths
{
	/// <summary>
	/// Axis-aligned bounding box. Box3 consists of a position, a size, and
	/// several utility functions. It is typically used for fast overlap tests.
	/// </summary>
	[Serializable]
	[StructLayout( LayoutKind.Sequential )]
	public struct Box3 : IEquatable<Box3>
	{
		private Vector3 _position;
		private Vector3 _size;

		/// <summary>
		/// Beginning corner. Typically has values lower than <see cref="End"/>.
		/// </summary>
		/// <value>Directly uses a private field.</value>
		public Vector3 Position
		{
			readonly get { return _position; }
			set { _position = value; }
		}

		/// <summary>
		/// Size from <see cref="Position"/> to <see cref="End"/>. Typically all components are positive.
		/// If the size is negative, you can use <see cref="Abs"/> to fix it.
		/// </summary>
		/// <value>Directly uses a private field.</value>
		public Vector3 Size
		{
			readonly get { return _size; }
			set { _size = value; }
		}

		/// <summary>
		/// Ending corner. This is calculated as <see cref="Position"/> plus
		/// <see cref="Size"/>. Setting this value will change the size.
		/// </summary>
		/// <value>
		/// Getting is equivalent to <paramref name="value"/> = <see cref="Position"/> + <see cref="Size"/>,
		/// setting is equivalent to <see cref="Size"/> = <paramref name="value"/> - <see cref="Position"/>
		/// </value>
		public Vector3 End
		{
			readonly get { return _position + _size; }
			set { _size = value - _position; }
		}

		/// <summary>
		/// The volume of this <see cref="Box3"/>.
		/// See also <see cref="HasVolume"/>.
		/// </summary>
		public readonly float Volume
		{
			get { return _size.X * _size.Y * _size.Z; }
		}

		/// <summary>
		/// Returns an <see cref="Box3"/> with equivalent position and size, modified so that
		/// the most-negative corner is the origin and the size is positive.
		/// </summary>
		/// <returns>The modified <see cref="Box3"/>.</returns>
		public readonly Box3 Abs()
		{
			Vector3 end = End;
			Vector3 topLeft = new Vector3( MathF.Min( _position.X, end.X ), MathF.Min( _position.Y, end.Y ), MathF.Min( _position.Z, end.Z ) );
			return new Box3( topLeft, _size.Abs() );
		}

		/// <summary>
		/// Returns the center of the <see cref="Box3"/>, which is equal
		/// to <see cref="Position"/> + (<see cref="Size"/> / 2).
		/// </summary>
		/// <returns>The center.</returns>
		public readonly Vector3 GetCenter()
		{
			return _position + (_size * 0.5f);
		}

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="Box3"/> completely encloses another one.
		/// </summary>
		/// <param name="with">The other <see cref="Box3"/> that may be enclosed.</param>
		/// <returns>
		/// A <see langword="bool"/> for whether or not this <see cref="Box3"/> encloses <paramref name="with"/>.
		/// </returns>
		public readonly bool Encloses( Box3 with )
		{
			Vector3 srcMin = _position;
			Vector3 srcMax = _position + _size;
			Vector3 dstMin = with._position;
			Vector3 dstMax = with._position + with._size;

			return srcMin.X <= dstMin.X &&
				   srcMax.X >= dstMax.X &&
				   srcMin.Y <= dstMin.Y &&
				   srcMax.Y >= dstMax.Y &&
				   srcMin.Z <= dstMin.Z &&
				   srcMax.Z >= dstMax.Z;
		}

		/// <summary>
		/// Returns this <see cref="Box3"/> expanded to include a given point.
		/// </summary>
		/// <param name="point">The point to include.</param>
		/// <returns>The expanded <see cref="Box3"/>.</returns>
		public readonly Box3 Expand( Vector3 point )
		{
			Vector3 begin = _position;
			Vector3 end = _position + _size;

			if ( point.X < begin.X )
			{
				begin.X = point.X;
			}
			if ( point.Y < begin.Y )
			{
				begin.Y = point.Y;
			}
			if ( point.Z < begin.Z )
			{
				begin.Z = point.Z;
			}

			if ( point.X > end.X )
			{
				end.X = point.X;
			}
			if ( point.Y > end.Y )
			{
				end.Y = point.Y;
			}
			if ( point.Z > end.Z )
			{
				end.Z = point.Z;
			}

			return new Box3( begin, end - begin );
		}

		/// <summary>
		/// Gets the position of one of the 8 endpoints of the <see cref="Box3"/>.
		/// </summary>
		/// <param name="idx">Which endpoint to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="idx"/> is less than 0 or greater than 7.
		/// </exception>
		/// <returns>An endpoint of the <see cref="Box3"/>.</returns>
		public readonly Vector3 GetEndpoint( int idx )
		{
			switch ( idx )
			{
				case 0:
					return new Vector3( _position.X, _position.Y, _position.Z );
				case 1:
					return new Vector3( _position.X, _position.Y, _position.Z + _size.Z );
				case 2:
					return new Vector3( _position.X, _position.Y + _size.Y, _position.Z );
				case 3:
					return new Vector3( _position.X, _position.Y + _size.Y, _position.Z + _size.Z );
				case 4:
					return new Vector3( _position.X + _size.X, _position.Y, _position.Z );
				case 5:
					return new Vector3( _position.X + _size.X, _position.Y, _position.Z + _size.Z );
				case 6:
					return new Vector3( _position.X + _size.X, _position.Y + _size.Y, _position.Z );
				case 7:
					return new Vector3( _position.X + _size.X, _position.Y + _size.Y, _position.Z + _size.Z );
				default:
					{
						throw new ArgumentOutOfRangeException( nameof( idx ),
							$"Index is {idx}, but a value from 0 to 7 is expected." );
					}
			}
		}

		/// <summary>
		/// Returns the normalized longest axis of the <see cref="Box3"/>.
		/// </summary>
		/// <returns>A vector representing the normalized longest axis of the <see cref="Box3"/>.</returns>
		public readonly Vector3 GetLongestAxis()
		{
			var axis = new Vector3( 1f, 0f, 0f );
			float maxSize = _size.X;

			if ( _size.Y > maxSize )
			{
				axis = new Vector3( 0f, 1f, 0f );
				maxSize = _size.Y;
			}

			if ( _size.Z > maxSize )
			{
				axis = new Vector3( 0f, 0f, 1f );
			}

			return axis;
		}

		/// <summary>
		/// Returns the <see cref="Axis"/> index of the longest axis of the <see cref="Box3"/>.
		/// </summary>
		/// <returns>A <see cref="Axis"/> index for which axis is longest.</returns>
		public readonly Axis GetLongestAxisIndex()
		{
			var axis = Axis.X;
			float maxSize = _size.X;

			if ( _size.Y > maxSize )
			{
				axis = Axis.Y;
				maxSize = _size.Y;
			}

			if ( _size.Z > maxSize )
			{
				axis = Axis.Z;
			}

			return axis;
		}

		/// <summary>
		/// Returns the scalar length of the longest axis of the <see cref="Box3"/>.
		/// </summary>
		/// <returns>The scalar length of the longest axis of the <see cref="Box3"/>.</returns>
		public readonly float GetLongestAxisSize()
		{
			float maxSize = _size.X;

			if ( _size.Y > maxSize )
				maxSize = _size.Y;

			if ( _size.Z > maxSize )
				maxSize = _size.Z;

			return maxSize;
		}

		/// <summary>
		/// Returns the normalized shortest axis of the <see cref="Box3"/>.
		/// </summary>
		/// <returns>A vector representing the normalized shortest axis of the <see cref="Box3"/>.</returns>
		public readonly Vector3 GetShortestAxis()
		{
			var axis = new Vector3( 1f, 0f, 0f );
			float maxSize = _size.X;

			if ( _size.Y < maxSize )
			{
				axis = new Vector3( 0f, 1f, 0f );
				maxSize = _size.Y;
			}

			if ( _size.Z < maxSize )
			{
				axis = new Vector3( 0f, 0f, 1f );
			}

			return axis;
		}

		/// <summary>
		/// Returns the <see cref="Axis"/> index of the shortest axis of the <see cref="Box3"/>.
		/// </summary>
		/// <returns>A <see cref="Axis"/> index for which axis is shortest.</returns>
		public readonly Axis GetShortestAxisIndex()
		{
			Axis axis = Axis.X;
			float maxSize = _size.X;

			if ( _size.Y < maxSize )
			{
				axis = Axis.Y;
				maxSize = _size.Y;
			}

			if ( _size.Z < maxSize )
			{
				axis = Axis.Z;
			}

			return axis;
		}

		/// <summary>
		/// Returns the scalar length of the shortest axis of the <see cref="Box3"/>.
		/// </summary>
		/// <returns>The scalar length of the shortest axis of the <see cref="Box3"/>.</returns>
		public readonly float GetShortestAxisSize()
		{
			float maxSize = _size.X;

			if ( _size.Y < maxSize )
				maxSize = _size.Y;

			if ( _size.Z < maxSize )
				maxSize = _size.Z;

			return maxSize;
		}

		/// <summary>
		/// Returns the support point in a given direction.
		/// This is useful for collision detection algorithms.
		/// </summary>
		/// <param name="dir">The direction to find support for.</param>
		/// <returns>A vector representing the support.</returns>
		public readonly Vector3 GetSupport( Vector3 dir )
		{
			Vector3 halfExtents = _size * 0.5f;
			Vector3 ofs = _position + halfExtents;

			return ofs + new Vector3(
				dir.X > 0f ? -halfExtents.X : halfExtents.X,
				dir.Y > 0f ? -halfExtents.Y : halfExtents.Y,
				dir.Z > 0f ? -halfExtents.Z : halfExtents.Z );
		}

		/// <summary>
		/// Returns a copy of the <see cref="Box3"/> grown a given amount of units towards all the sides.
		/// </summary>
		/// <param name="by">The amount to grow by.</param>
		/// <returns>The grown <see cref="Box3"/>.</returns>
		public readonly Box3 Grow( float by )
		{
			Box3 res = this;

			res._position.X -= by;
			res._position.Y -= by;
			res._position.Z -= by;
			res._size.X += 2.0f * by;
			res._size.Y += 2.0f * by;
			res._size.Z += 2.0f * by;

			return res;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Box3"/> contains a point,
		/// or <see langword="false"/> otherwise.
		/// </summary>
		/// <param name="point">The point to check.</param>
		/// <returns>
		/// A <see langword="bool"/> for whether or not the <see cref="Box3"/> contains <paramref name="point"/>.
		/// </returns>
		public readonly bool HasPoint( Vector3 point )
		{
			if ( point.X < _position.X )
				return false;
			if ( point.Y < _position.Y )
				return false;
			if ( point.Z < _position.Z )
				return false;
			if ( point.X > _position.X + _size.X )
				return false;
			if ( point.Y > _position.Y + _size.Y )
				return false;
			if ( point.Z > _position.Z + _size.Z )
				return false;

			return true;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Box3"/>
		/// has a surface or a length, and <see langword="false"/>
		/// if the <see cref="Box3"/> is empty (all components
		/// of <see cref="Size"/> are zero or negative).
		/// </summary>
		/// <returns>
		/// A <see langword="bool"/> for whether or not the <see cref="Box3"/> has surface.
		/// </returns>
		public readonly bool HasSurface()
		{
			return _size.X > 0.0f || _size.Y > 0.0f || _size.Z > 0.0f;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Box3"/> has
		/// area, and <see langword="false"/> if the <see cref="Box3"/>
		/// is linear, empty, or has a negative <see cref="Size"/>.
		/// See also <see cref="Volume"/>.
		/// </summary>
		/// <returns>
		/// A <see langword="bool"/> for whether or not the <see cref="Box3"/> has volume.
		/// </returns>
		public readonly bool HasVolume()
		{
			return _size.X > 0.0f && _size.Y > 0.0f && _size.Z > 0.0f;
		}

		/// <summary>
		/// Returns the intersection of this <see cref="Box3"/> and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other <see cref="Box3"/>.</param>
		/// <returns>The clipped <see cref="Box3"/>.</returns>
		public readonly Box3 Intersection( Box3 with )
		{
			Vector3 srcMin = _position;
			Vector3 srcMax = _position + _size;
			Vector3 dstMin = with._position;
			Vector3 dstMax = with._position + with._size;

			Vector3 min, max;

			if ( srcMin.X > dstMax.X || srcMax.X < dstMin.X )
			{
				return new Box3();
			}

			min.X = srcMin.X > dstMin.X ? srcMin.X : dstMin.X;
			max.X = srcMax.X < dstMax.X ? srcMax.X : dstMax.X;

			if ( srcMin.Y > dstMax.Y || srcMax.Y < dstMin.Y )
			{
				return new Box3();
			}

			min.Y = srcMin.Y > dstMin.Y ? srcMin.Y : dstMin.Y;
			max.Y = srcMax.Y < dstMax.Y ? srcMax.Y : dstMax.Y;

			if ( srcMin.Z > dstMax.Z || srcMax.Z < dstMin.Z )
			{
				return new Box3();
			}

			min.Z = srcMin.Z > dstMin.Z ? srcMin.Z : dstMin.Z;
			max.Z = srcMax.Z < dstMax.Z ? srcMax.Z : dstMax.Z;

			return new Box3( min, max - min );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Box3"/> overlaps with <paramref name="with"/>
		/// (i.e. they have at least one point in common).
		/// </summary>
		/// <param name="with">The other <see cref="Box3"/> to check for intersections with.</param>
		/// <returns>
		/// A <see langword="bool"/> for whether or not they are intersecting.
		/// </returns>
		public readonly bool Intersects( Box3 with )
		{
			if ( _position.X >= with._position.X + with._size.X )
				return false;
			if ( _position.X + _size.X <= with._position.X )
				return false;
			if ( _position.Y >= with._position.Y + with._size.Y )
				return false;
			if ( _position.Y + _size.Y <= with._position.Y )
				return false;
			if ( _position.Z >= with._position.Z + with._size.Z )
				return false;
			if ( _position.Z + _size.Z <= with._position.Z )
				return false;

			return true;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Box3"/> is on both sides of <paramref name="plane"/>.
		/// </summary>
		/// <param name="plane">The <see cref="Plane"/> to check for intersection.</param>
		/// <returns>
		/// A <see langword="bool"/> for whether or not the <see cref="Box3"/> intersects the <see cref="Plane"/>.
		/// </returns>
		public readonly bool IntersectsPlane( Plane plane )
		{
			Vector3[] points =
			{
				new Vector3(_position.X, _position.Y, _position.Z),
				new Vector3(_position.X, _position.Y, _position.Z + _size.Z),
				new Vector3(_position.X, _position.Y + _size.Y, _position.Z),
				new Vector3(_position.X, _position.Y + _size.Y, _position.Z + _size.Z),
				new Vector3(_position.X + _size.X, _position.Y, _position.Z),
				new Vector3(_position.X + _size.X, _position.Y, _position.Z + _size.Z),
				new Vector3(_position.X + _size.X, _position.Y + _size.Y, _position.Z),
				new Vector3(_position.X + _size.X, _position.Y + _size.Y, _position.Z + _size.Z)
			};

			bool over = false;
			bool under = false;

			for ( int i = 0; i < 8; i++ )
			{
				if ( plane.DistanceTo( points[i] ) > 0 )
				{
					over = true;
				}
				else
				{
					under = true;
				}
			}

			return under && over;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Box3"/> intersects
		/// the line segment between <paramref name="from"/> and <paramref name="to"/>.
		/// </summary>
		/// <param name="from">The start of the line segment.</param>
		/// <param name="to">The end of the line segment.</param>
		/// <returns>
		/// A <see langword="bool"/> for whether or not the <see cref="Box3"/> intersects the line segment.
		/// </returns>
		public readonly bool IntersectsSegment( Vector3 from, Vector3 to )
		{
			float min = 0f;
			float max = 1f;

			for ( int i = 0; i < 3; i++ )
			{
				float segFrom = from.At( i );
				float segTo = to.At( i );
				float boxBegin = _position.At( i );
				float boxEnd = boxBegin + _size.At( i );
				float cmin, cmax;

				if ( segFrom < segTo )
				{
					if ( segFrom > boxEnd || segTo < boxBegin )
					{
						return false;
					}

					float length = segTo - segFrom;
					cmin = segFrom < boxBegin ? (boxBegin - segFrom) / length : 0f;
					cmax = segTo > boxEnd ? (boxEnd - segFrom) / length : 1f;
				}
				else
				{
					if ( segTo > boxEnd || segFrom < boxBegin )
					{
						return false;
					}

					float length = segTo - segFrom;
					cmin = segFrom > boxEnd ? (boxEnd - segFrom) / length : 0f;
					cmax = segTo < boxBegin ? (boxBegin - segFrom) / length : 1f;
				}

				if ( cmin > min )
				{
					min = cmin;
				}

				if ( cmax < max )
				{
					max = cmax;
				}
				if ( max < min )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="Box3"/> is finite, by calling
		/// <see cref="MathF.IsFinite(float)"/> on each component.
		/// </summary>
		/// <returns>Whether this vector is finite or not.</returns>
		public readonly bool IsFinite()
		{
			return _position.IsFinite() && _size.IsFinite();
		}

		/// <summary>
		/// Returns a larger <see cref="Box3"/> that contains this <see cref="Box3"/> and <paramref name="with"/>.
		/// </summary>
		/// <param name="with">The other <see cref="Box3"/>.</param>
		/// <returns>The merged <see cref="Box3"/>.</returns>
		public readonly Box3 Merge( Box3 with )
		{
			Vector3 beg1 = _position;
			Vector3 beg2 = with._position;
			var end1 = new Vector3( _size.X, _size.Y, _size.Z ) + beg1;
			var end2 = new Vector3( with._size.X, with._size.Y, with._size.Z ) + beg2;

			var min = new Vector3(
				beg1.X < beg2.X ? beg1.X : beg2.X,
				beg1.Y < beg2.Y ? beg1.Y : beg2.Y,
				beg1.Z < beg2.Z ? beg1.Z : beg2.Z
			);

			var max = new Vector3(
				end1.X > end2.X ? end1.X : end2.X,
				end1.Y > end2.Y ? end1.Y : end2.Y,
				end1.Z > end2.Z ? end1.Z : end2.Z
			);

			return new Box3( min, max - min );
		}

		/// <summary>
		/// Constructs an <see cref="Box3"/> from a position and size.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="size">The size, typically positive.</param>
		public Box3( Vector3 position, Vector3 size )
		{
			_position = position;
			_size = size;
		}

		/// <summary>
		/// Constructs an <see cref="Box3"/> from a <paramref name="position"/>,
		/// <paramref name="width"/>, <paramref name="height"/>, and <paramref name="depth"/>.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="width">The width, typically positive.</param>
		/// <param name="height">The height, typically positive.</param>
		/// <param name="depth">The depth, typically positive.</param>
		public Box3( Vector3 position, float width, float height, float depth )
		{
			_position = position;
			_size = new Vector3( width, height, depth );
		}

		/// <summary>
		/// Constructs an <see cref="Box3"/> from <paramref name="x"/>,
		/// <paramref name="y"/>, <paramref name="z"/>, and <paramref name="size"/>.
		/// </summary>
		/// <param name="x">The position's X coordinate.</param>
		/// <param name="y">The position's Y coordinate.</param>
		/// <param name="z">The position's Z coordinate.</param>
		/// <param name="size">The size, typically positive.</param>
		public Box3( float x, float y, float z, Vector3 size )
		{
			_position = new Vector3( x, y, z );
			_size = size;
		}

		/// <summary>
		/// Constructs an <see cref="Box3"/> from <paramref name="x"/>,
		/// <paramref name="y"/>, <paramref name="z"/>, <paramref name="width"/>,
		/// <paramref name="height"/>, and <paramref name="depth"/>.
		/// </summary>
		/// <param name="x">The position's X coordinate.</param>
		/// <param name="y">The position's Y coordinate.</param>
		/// <param name="z">The position's Z coordinate.</param>
		/// <param name="width">The width, typically positive.</param>
		/// <param name="height">The height, typically positive.</param>
		/// <param name="depth">The depth, typically positive.</param>
		public Box3( float x, float y, float z, float width, float height, float depth )
		{
			_position = new Vector3( x, y, z );
			_size = new Vector3( width, height, depth );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the AABBs are exactly equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="left">The left AABB.</param>
		/// <param name="right">The right AABB.</param>
		/// <returns>Whether or not the AABBs are exactly equal.</returns>
		public static bool operator ==( Box3 left, Box3 right )
		{
			return left.Equals( right );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the AABBs are not equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="left">The left AABB.</param>
		/// <param name="right">The right AABB.</param>
		/// <returns>Whether or not the AABBs are not equal.</returns>
		public static bool operator !=( Box3 left, Box3 right )
		{
			return !left.Equals( right );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the AABB is exactly equal
		/// to the given object (<paramref name="obj"/>).
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Whether or not the AABB and the object are equal.</returns>
		public override readonly bool Equals( [NotNullWhen( true )] object? obj )
		{
			return obj is Box3 other && Equals( other );
		}

		/// <summary>
		/// Returns <see langword="true"/> if the AABBs are exactly equal.
		/// Note: Due to floating-point precision errors, consider using
		/// <see cref="IsEqualApprox"/> instead, which is more reliable.
		/// </summary>
		/// <param name="other">The other AABB.</param>
		/// <returns>Whether or not the AABBs are exactly equal.</returns>
		public readonly bool Equals( Box3 other )
		{
			return _position == other._position && _size == other._size;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this AABB and <paramref name="other"/> are approximately equal,
		/// by running <see cref="Vector3.IsEqualApprox(Vector3)"/> on each component.
		/// </summary>
		/// <param name="other">The other AABB to compare.</param>
		/// <returns>Whether or not the AABBs structures are approximately equal.</returns>
		public readonly bool IsEqualApprox( Box3 other )
		{
			return _position.IsEqualApprox( other._position ) && _size.IsEqualApprox( other._size );
		}

		/// <summary>
		/// Serves as the hash function for <see cref="Box3"/>.
		/// </summary>
		/// <returns>A hash code for this AABB.</returns>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine( _position, _size );
		}

		/// <summary>
		/// Converts this <see cref="Box3"/> to a string.
		/// </summary>
		/// <returns>A string representation of this AABB.</returns>
		public override readonly string ToString()
		{
			return $"{_position}, {_size}";
		}

		/// <summary>
		/// Converts this <see cref="Box3"/> to a string with the given <paramref name="format"/>.
		/// </summary>
		/// <returns>A string representation of this AABB.</returns>
		public readonly string ToString( string? format )
		{
			return $"{_position.ToString( format )}, {_size.ToString( format )}";
		}
	}
}
