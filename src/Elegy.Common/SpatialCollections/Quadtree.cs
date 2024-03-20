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
		/// 
		/// </summary>
		/// <param name="rootBound"></param>
		/// <param name="items"></param>
		public Quadtree( Rect2 rootBound, IReadOnlyList<TItem> items )
			: base( rootBound, items, 2 )
		{
			// TODO: implement
			throw new NotImplementedException();
		}

		// TODO: maybe some raycasting or radius search functions specific to quadtrees!
	}
}
