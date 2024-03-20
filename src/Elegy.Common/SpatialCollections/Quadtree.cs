// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.SpatialCollections
{
	/// <summary>
	/// A quadtree. Dunno what you'd use it for, vegetation maybe.
	/// </summary>
	public class Quadtree<TItem> : STree<Rect2, TItem>
	{
		/// <summary>
		/// Constructs a quadtree.
		/// </summary>
		/// <param name="rootBound">
		/// The bounding box of the tree's root.
		/// </param>
		/// <param name="items">
		/// A list of items of the tree. Can be empty for a start, as the tree is only constructed later.
		/// </param>
		/// <param name="isInBoundMethod">
		/// A method that checks whether or not an AABB of an octree node intersects with an item.
		/// Look at <see cref="STreeUtilities.IsInBoundVector2"/>.
		/// </param>
		/// <param name="shouldSubdivideMethod">
		/// A method that says whether or not a given node should subdivide.
		/// Look at <see cref="STreeUtilities.ShouldSubdivideThreshold{TBound, TItem}(int)"/>.
		/// </param>
		/// <param name="onlyFirstIntersection">
		/// When collecting intersections, one (<c>true</c>) or multiple (<c>false</c>) children will receive a reference to a given item.
		/// If you have geometric objects that happen to be inside multiple nodes, this is pretty relevant.
		/// </param>
		public Quadtree( Rect2 rootBound, IReadOnlyList<TItem> items,
			IsInBoundFn isInBoundMethod,
			ShouldSubdivideFn shouldSubdivideMethod,
			bool onlyFirstIntersection = false )
			: base( rootBound, items, 2,
				isInBound: isInBoundMethod,
				shouldSubdivide: shouldSubdivideMethod,
				getChildVolume: STreeUtilities.GetChildVolumeQuadtree<TItem>(),
				collectIntersections: onlyFirstIntersection
				? STreeUtilities.CollectIntersectionsFirst( isInBoundMethod )
				: STreeUtilities.CollectIntersectionsAll( isInBoundMethod ) )
		{
		}

		// TODO: maybe some raycasting or radius search functions specific to quadtrees!
	}
}
