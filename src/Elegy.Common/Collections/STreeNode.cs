// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Collections
{
	/// <summary>
	/// N-dimensional tree node, stores a list of <typeparamref name="TItem"/>s
	/// from its parent tree that intersect with its <typeparamref name="TBound"/>.
	/// </summary>
	/// <typeparam name="TBound">
	/// For example: <seealso cref="Aabb"/> or <seealso cref="Rect2"/>.
	/// </typeparam>
	/// <typeparam name="TItem">
	/// For example: <seealso cref="Vector3"/> or anything, really!
	/// </typeparam>
	public class STreeNode<TBound, TItem> where TBound : struct
	{
		#region Private fields
		private bool mIsLeaf = true;
		private readonly int mDimensions;
		private TBound mBound;
		private List<int> mItemIndices = new();
		private STreeNode<TBound, TItem>[] mChildren = Array.Empty<STreeNode<TBound, TItem>>();
		#endregion

		#region Properties
		/// <summary>
		///  How many child nodes there can be.
		/// </summary>
		public int Combinations => 1 << Dimensions;

		/// <summary>
		/// The dimensionality of this node.
		/// </summary>
		public int Dimensions => mDimensions;

		/// <summary>
		/// Number of items currently linked to this node.
		/// </summary>
		public int NumItems => mItemIndices.Count;

		/// <summary>
		/// The bounding volume of this node.
		/// </summary>
		public TBound Bound => mBound;

		/// <summary>
		/// The items of this node.
		/// </summary>
		public IReadOnlyList<int> ItemIndices => mItemIndices;

		/// <summary>
		/// This node's children.
		/// </summary>
		public STreeNode<TBound, TItem>[] Children => mChildren;
		#endregion

		/// <summary>
		/// The constructor of this tree node.
		/// </summary>
		/// <param name="bound">
		/// The bounding volume to bind a subset of items to.
		/// </param>
		/// <param name="dimensions">
		/// Dimensionality of the tree. (2 or 3 usually)
		/// </param>
		public STreeNode(TBound bound, int dimensions )
		{
			mDimensions = dimensions;
			mBound = bound;
		}

		/// <summary>
		/// Adds an item to the node. The <paramref name="unique"/> parameter can 
		/// affect performance and is needless if your indices are guaranteed to be unique.
		/// </summary>
		public void Add( int itemIndex, bool unique = false )
		{
			if ( unique )
			{
				if ( mItemIndices.Contains( itemIndex ) )
				{
					return;
				}
			}

			mItemIndices.Add( itemIndex );
		}

		/// <summary>
		/// Creates children for this node, not recursive though!
		/// </summary>
		public void CreateChildren( List<STreeNode<TBound, TItem>> treeNodes,
			STree<TBound, TItem>.GetChildVolumeFn boundSubdivisionMethod )
		{
			mChildren = new STreeNode<TBound, TItem>[1 << Dimensions];
			for ( int i = 0; i < Combinations; i++ )
			{
				mChildren[i] = new( boundSubdivisionMethod( mBound, i ), Dimensions );
				treeNodes.Add( mChildren[i] );
			}

			// Become a non-leaf because you have children now
			mIsLeaf = false;
		}

		/// <summary>
		/// Returns whether this node is a leaf or not.
		/// </summary>
		public bool IsLeaf()
		{
			return mIsLeaf;
		}

		/// <summary>
		/// Returns whether this node is empty.
		/// </summary>
		public bool IsEmpty()
		{
			return NumItems == 0;
		}

		/// <summary>
		/// Executes the <paramref name="method"/> for each child node.
		/// </summary>
		public void ForEachChildNode( Action<STreeNode<TBound, TItem>> method )
		{
			if ( IsLeaf() )
			{
				return;
			}

			for ( int i = 0; i < Dimensions; i++ )
			{
				method( mChildren[i] );
			}
		}

		/// <summary>
		/// Executes the <paramref name="method"/> for each item.
		/// </summary>
		public void ForEachItem( IReadOnlyList<TItem> treeItems, Action<TItem> method )
		{
			for ( int i = 0; i < NumItems; i++ )
			{
				method( treeItems[mItemIndices[i]] );
			}
		}
	}
}
