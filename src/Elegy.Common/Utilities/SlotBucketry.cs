// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Utilities
{
	/// <summary>
	/// Expandable array of reusable element slots.
	/// Superfast deletion, fast insertion. Don't ask why I called it bucketry of all things.
	/// </summary>
	public class SlotBucketry<T> where T: new()
	{
		private const int FlagsPerElement = sizeof( ulong ) * 8;
		private int mElementsPerBucket;

		private T[] mArray;
		private ulong[] mFlagArray;

		/// <summary>
		/// Creates a new <see cref="SlotBucketry{T}"/>.
		/// </summary>
		/// <param name="bucketSizeIn64">Multiplier of bucket size. Default is 1x64.</param>
		/// <param name="numInitialBuckets">Initial amount of buckets.</param>
		public SlotBucketry( int bucketSizeIn64 = 1, int numInitialBuckets = 1 )
		{
			mElementsPerBucket = bucketSizeIn64 * sizeof( ulong );

			mArray = new T[mElementsPerBucket * numInitialBuckets];
			mFlagArray = new ulong[bucketSizeIn64 * numInitialBuckets];
		}

		/// <inheritdoc/>
		public T this[int index]
		{
			get => mArray[index];
			set => mArray[index] = value;
		}

		/// <summary>
		/// Gets a reference to a <typeparamref name="T"/> at <paramref name="index"/>.
		/// </summary>
		public ref T GetRefOf( int index )
		{
			return ref mArray[index];
		}

		/// <summary>
		/// How many elements are allocated per "bucket".
		/// </summary>
		public int ElementsPerBucket => mElementsPerBucket;

		/// <summary>
		/// How many buckets are currently allocated?
		/// </summary>
		public int Buckets => mArray.Length / ElementsPerBucket;

		/// <summary>
		/// Provide the RAW ARRAY ITSELF because <see cref="List{T}"/> doesn't
		/// do that for some reason and yet it's very useful >:(
		/// </summary>
		public T[] InternalArray => mArray;

		/// <inheritdoc/>
		public void Clear()
		{
			Array.Fill( mFlagArray, 0UL );
		}

		/// <inheritdoc/>
		public void CopyTo( T[] array, int arrayIndex )
		{
			Array.Copy( mArray, 0, array, arrayIndex, mArray.Length );
		}

		/// <summary>
		/// Expands the list to support another <paramref name="numElements"/> elements.
		/// </summary>
		public void Expand( int numElements )
			=> ExpandBuckets( (numElements / ElementsPerBucket) + 1 );

		/// <summary>
		/// Expands the list by another <paramref name="numBuckets"/> buckets.
		/// </summary>
		public void ExpandBuckets( int numBuckets )
		{
			int newAmount = (Buckets + numBuckets) * ElementsPerBucket;

			Array.Resize( ref mArray, newAmount );
			Array.Resize( ref mFlagArray, newAmount / FlagsPerElement );
		}

		#region Element stuff
		/// <summary>
		/// Creates a new <typeparamref name="T"/> at the next free slot
		/// and returns a reference to it.
		/// </summary>
		public ref T CreateElement()
		{
			int freeIndex = FindFreeIndex();
			if ( freeIndex == -1 )
			{
				ExpandBuckets( 1 );
				// Speed optimisation: we already know that this slot will be free to take
				freeIndex = (Buckets - 1) * ElementsPerBucket;
			}

			mArray[freeIndex] = new T();
			SetIndexFlag( freeIndex, true );

			return ref mArray[freeIndex];
		}

		/// <summary>
		/// Frees up the slot at <paramref name="elementIndex"/>.
		/// </summary>
		public bool FreeElement( int elementIndex )
		{
			bool oldValue = GetIndexFlag( elementIndex );
			SetIndexFlag( elementIndex, false );

			return oldValue;
		}
		#endregion

		#region Index stuff
		public void SetIndexFlag( int elementIndex, bool value )
		{
			int flagIndex = elementIndex / FlagsPerElement;
			int position = elementIndex % FlagsPerElement;

			if ( value )
			{
				mFlagArray[flagIndex] |= 1U << position;
			}
			else
			{
				mFlagArray[flagIndex] &= ~(1U << position);
			}
		}

		public bool GetIndexFlag( int elementIndex )
		{
			int flagIndex = elementIndex / FlagsPerElement;
			int position = elementIndex % FlagsPerElement;

			return (mFlagArray[flagIndex] & (1U << position)) != 0;
		}

		public int FindFreeIndex()
		{
			for ( int i = 0; i < mArray.Length / sizeof( ulong ); i++ )
			{
				ulong flags = mFlagArray[i];
				if ( flags == ulong.MaxValue )
				{
					continue;
				}

				if ( flags == 0 )
				{
					return i * sizeof( ulong );
				}

				for ( int bit = 0; bit < sizeof( ulong ); bit++ )
				{
					if ( (flags & (1UL << bit)) == 0 )
					{
						return i * sizeof( ulong ) + bit;
					}
				}
			}

			return -1;
		}

		public bool HasFreeIndices()
		{
			for ( int i = 0; i < mArray.Length / sizeof( ulong ); i++ )
			{
				if ( mFlagArray[i] != ulong.MaxValue )
				{
					return true;
				}
			}

			return false;
		}

		public bool HasFreeIndicesInRegion( int elementIndex )
		{
			int flagIndex = elementIndex / FlagsPerElement;
			return mFlagArray[flagIndex] != 0;
		}
		#endregion
	}
}
