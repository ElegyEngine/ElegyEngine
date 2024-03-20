﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.SpatialCollections
{
	/// <summary>
	/// Built-in helper methods for <seealso cref="STree{TBound, TItem}"/>.
	/// </summary>
	public static class STreeUtilities
	{
		#region IsInBoundFn methods
		/// <summary>
		/// TBound: <see cref="Box3"/>
		/// TItem: <see cref="Vector3"/>
		/// </summary>
		public static STree<Box3, Vector3>.IsInBoundFn IsInBoundVector3()
		{
			return ( in Vector3 item, in Box3 bound ) =>
			{
				return bound.HasPoint( item );
			};
		}

		/// <summary>
		/// TBound: <see cref="Box3"/>
		/// TItem: <see cref="Vector4"/>
		/// </summary>
		public static STree<Box3, Vector4>.IsInBoundFn IsInBoundVector4()
		{
			return ( in Vector4 item, in Box3 bound ) =>
			{
				return bound.HasPoint( item.ToVector3() );
			};
		}
		#endregion

		#region ShouldSubdivideFn methods
		/// <summary>
		/// Very simple threshold-based subdivision function.
		/// If a node has more than <paramref name="threshold"/> elements, it'll subdivide.
		/// If not, it'll become a leaf.
		/// </summary>
		public static STree<TBound, TItem>.ShouldSubdivideFn ShouldSubdivideThreshold<TBound, TItem>( int threshold ) where TBound : struct
		{
			return ( in STreeNode<TBound, TItem> node ) =>
			{
				return node.NumItems > threshold;
			};
		}
		#endregion

		#region GetChildVolumeFn methods
		/// <summary>
		/// Look at <see cref="GetChildVolumeOctree{TItem}"/> to see the usage of this.
		/// </summary>
		public static readonly int[] OctreeMinMaxIndices =
		{
			0, 0, 0,
			0, 0, 1,
			0, 1, 0,
			0, 1, 1,
			1, 0, 0,
			1, 0, 1,
			1, 1, 0,
			1, 1, 1
		};

		/// <summary>
		/// Subdivides an AABB into 8 equal octants.
		/// Restriction: dimensionality must be 3.
		/// </summary>
		public static STree<Box3, TItem>.GetChildVolumeFn GetChildVolumeOctree<TItem>()
		{
			return ( in Box3 parentBox, in int childIndex ) =>
			{
				Vector3[] sizes = { parentBox.Position, parentBox.End };

				Vector3 centre = parentBox.GetCenter();
				Vector3 extent = new Vector3()
				{
					X = sizes[OctreeMinMaxIndices[childIndex * 3 + 0]].X,
					Y = sizes[OctreeMinMaxIndices[childIndex * 3 + 1]].Y,
					Z = sizes[OctreeMinMaxIndices[childIndex * 3 + 2]].Z
				};

				return new Box3( centre, extent );
			};
		}
		#endregion

		#region CollectIntersectionsFn methods
		/// <summary>
		/// Accumulates all intersections into a node's children. Meaning if your item intersects all children, all children get that item.
		/// </summary>
		public static STree<Box3, TItem>.CollectIntersectionsFn CollectIntersectionsOctreeAll<TItem>( STree<Box3, TItem>.IsInBoundFn intersectionMethod )
		{
			return ( in STreeNode<Box3, TItem> parentNode, in TItem item, out int[] hits ) =>
			{
				int numHits = 0;
				Span<int> hitSpan = stackalloc int[parentNode.Combinations];
				
				for ( int i = 0; i < parentNode.Combinations; i++ )
				{
					if ( intersectionMethod( item, parentNode.Children[i].Bound ) )
					{
						hitSpan[numHits] = i;
						numHits++;
					}
				}

				if ( numHits == 0 )
				{
					hits = Array.Empty<int>();
					return false;
				}

				hits = hitSpan.Slice( 0, numHits ).ToArray();
				return true;
			};
		}

		/// <summary>
		/// Only takes in the first intersection.
		/// </summary>
		public static STree<Box3, TItem>.CollectIntersectionsFn CollectIntersectionsOctreeFirst<TItem>( STree<Box3, TItem>.IsInBoundFn intersectionMethod )
		{
			return ( in STreeNode<Box3, TItem> parentNode, in TItem item, out int[] hits ) =>
			{
				for ( int i = 0; i < parentNode.Combinations; i++ )
				{
					if ( intersectionMethod( item, parentNode.Children[i].Bound ) )
					{
						hits = new int[1] { i };
						return true;
					}
				}

				hits = Array.Empty<int>();
				return false;
			};
		}
		#endregion
	}
}
