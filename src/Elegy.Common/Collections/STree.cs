// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Collections
{
	/// <summary>
	/// N-dimensional spatial tree (s-tree) that stores <typeparamref name="TItem"/>s inside a <typeparamref name="TBound"/>.
	/// </summary>
	/// <typeparam name="TBound">
	/// For example: <seealso cref="Aabb"/> or <seealso cref="Rect2"/>.
	/// </typeparam>
	/// <typeparam name="TItem">
	/// For example: <seealso cref="Vector3"/> or anything, really!
	/// </typeparam>
	public class STree<TBound, TItem> where TBound : struct
	{
		#region Private fields
		protected int mDimensions;
		protected TBound mBound;
		protected List<TItem> mItems;
		protected List<STreeNode<TBound, TItem>> mNodes = new();
		protected List<STreeNode<TBound, TItem>> mLeaves = new();
		#endregion

		#region Delegates
		/// <summary>
		/// Does the item intersect the bound?
		/// </summary>
		public delegate bool IsInBoundFn( in TItem item, in TBound bound );

		/// <summary>
		/// With these elements loaded, should this node subdivide any further?
		/// </summary>
		public delegate bool ShouldSubdivideFn( in STreeNode<TBound, TItem> node );

		/// <summary>
		/// Get a subdivided bounding volume for the Nth child node
		/// </summary>
		public delegate TBound GetChildVolumeFn( in TBound bound, in int childIndex );

		/// <summary>
		/// If two or more intersections occur within the node's children, resolve them.
		/// </summary>
		/// <returns>True on successful resolution, false if there were no usable intersections at all.</returns>
		public delegate bool CollectIntersectionsFn( in STreeNode<TBound, TItem> parentNode, in TItem item, out int[] hits );
		#endregion

		#region Properties
		/// <summary>
		/// Usually **the** number of children that must be had.
		/// Technically the max number of children that this node may have.
		/// </summary>
		public int Combinations => 1 << Dimensions;

		/// <summary>
		/// Dimensionality of this tree (2 or 3 usually).
		/// </summary>
		public int Dimensions => mDimensions;

		/// <summary>
		/// Bounding volume of this tree.
		/// </summary>
		public TBound Bound => mBound;

		/// <summary>
		/// This tree's items.
		/// </summary>
		public IReadOnlyList<TItem> Items => mItems;

		/// <summary>
		/// All of the nodes belonging to this tree.
		/// </summary>
		public IReadOnlyList<STreeNode<TBound, TItem>> Nodes => mNodes;

		/// <summary>
		/// A subset of <c>Nodes</c> which are leaf nodes.
		/// </summary>
		public IReadOnlyList<STreeNode<TBound, TItem>> Leaves => mLeaves;

		/// <summary>
		/// See <see cref="IsInBoundFn"/>.
		/// </summary>
		public IsInBoundFn IsInBound { get; set; }

		/// <summary>
		/// See <see cref="ShouldSubdivideFn"/>.
		/// </summary>
		public ShouldSubdivideFn ShouldSubdivide { get; set; }

		/// <summary>
		/// See <see cref="GetChildVolumeFn"/>.
		/// </summary>
		public GetChildVolumeFn GetChildVolume { get; set; }

		/// <summary>
		/// See <see cref="CollectIntersectionsFn"/>.
		/// </summary>
		public CollectIntersectionsFn CollectIntersections { get; set; }
		#endregion

		/// <summary>
		/// The constructor for this thing.
		/// </summary>
		/// <param name="rootBound">The bounding volume of the root node.</param>
		/// <param name="items"></param>
		/// <param name="dimensions"></param>
		public STree( TBound rootBound, IReadOnlyList<TItem> items, int dimensions )
		{
			mItems = items.ToList();
			mBound = rootBound;
			mDimensions = dimensions;
		}

		/// <summary>
		/// Clears the tree.
		/// </summary>
		public void Clear()
		{
			mLeaves.Clear();
			mNodes.Clear();
			mItems.Clear();
		}

		/// <summary>
		/// Adds an item into the octree, without linking.
		/// If you'd like to rebuild the octree, call Build
		/// </summary>
		public void Add( TItem item ) => mItems.Add( item );

		/// <summary>
		/// Builds the tree.
		/// </summary>
		public void Build()
		{
			mLeaves.Clear();
			mNodes.Clear();

			STreeNode<TBound, TItem> node = new( mBound, mDimensions );
			mNodes.Add( node );

			// No elements, root node is empty
			if ( mItems.Count <= 0 )
			{
				return;
			}

			// Fill with items
			for ( int i = 0; i < mItems.Count; i++ )
			{
				if ( IsInBound( mItems[i], mBound ) )
				{
					node.Add( i );
				}
			}

			// Recursively build nodes in the tree
			BuildNode( node );

			// Now that the tree is built, extract the leaf nodes
			for ( int i = 0; i < mNodes.Count; i++ )
			{
				if ( mNodes[i].IsLeaf() )
				{
					mLeaves.Add( mNodes[i] );
				}
			}
		}

		/// <summary>
		/// Builds nodes recursively.
		/// </summary>
		protected void BuildNode( STreeNode<TBound, TItem> node )
		{
			// Bail out, no need to subdivide
			if ( !ShouldSubdivide( node ) )
			{
				return;
			}

			// The node can be subdivided, create the child nodes
			// and figure out which element belongs to which node
			int itemIndex = -1;
			node.CreateChildren( mNodes, GetChildVolume );
			node.ForEachItem( mItems, item =>
			{
				itemIndex++;

				if ( CollectIntersections( node, item, out int[] hits ) )
				{
					for ( int i = 0; i < hits.Length; i++ )
					{
						node.Children[hits[i]].Add( itemIndex );
					}
				}
			} );

			// Now that we've done that work, let's move down the tree
			node.ForEachChildNode( child =>
			{
				BuildNode( child );
			} );
		}
	}
}
