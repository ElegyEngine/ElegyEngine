﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.SpatialCollections
{
	/// <summary>
	/// An octree container.
	/// Elegy.MapCompiler uses them to quickly and roughly subdivide a level.
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	public class Octree<TItem> : STree<Box3, TItem>
	{
		/// <summary>
		/// Constructs an octree.
		/// </summary>
		/// <param name="rootBound">
		/// The bounding box of the tree's root.
		/// </param>
		/// <param name="items">
		/// A list of items of the tree. Can be empty for a start, as the tree is only constructed later.
		/// </param>
		/// <param name="isInBoundMethod">
		/// A method that checks whether or not an AABB of an octree node intersects with an item. Look at <see cref="STreeUtilities.IsInBoundVector3"/>.
		/// </param>
		/// <param name="shouldSubdivideMethod">
		/// A method that says whether or not a given node should subdivide. Look at <see cref="STreeUtilities.ShouldSubdivideThreshold{TBound, TItem}(int)"/>.
		/// </param>
		/// <param name="onlyFirstIntersection">
		/// When collecting intersections, one (<c>true</c>) or multiple (<c>false</c>) children will receive a reference to a given item.
		/// If you have geometric objects that happen to be inside multiple nodes, this is pretty relevant.
		/// </param>
		public Octree( Box3 rootBound, IReadOnlyList<TItem> items,
			IsInBoundFn isInBoundMethod,
			ShouldSubdivideFn shouldSubdivideMethod,
			bool onlyFirstIntersection = false )
			: base( rootBound, items, 3,
				isInBound: isInBoundMethod,
				shouldSubdivide: shouldSubdivideMethod,
				getChildVolume: STreeUtilities.GetChildVolumeOctree<TItem>(),
				collectIntersections: onlyFirstIntersection
				? STreeUtilities.CollectIntersectionsFirst( isInBoundMethod )
				: STreeUtilities.CollectIntersectionsAll( isInBoundMethod ) )
		{
		}

		// TODO: maybe some raycasting or radius search functions specific to octrees!
	}
}
