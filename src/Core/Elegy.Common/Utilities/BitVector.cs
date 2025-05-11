// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Elegy.Common.Utilities;

class BitVectorDebugProxy
{
	private BitVector mBitVector;

	public BitVectorDebugProxy( BitVector bitVector )
	{
		mBitVector = bitVector;
	}

	public bool[] Bits => mBitVector.InternalArray.SelectMany( l =>
		{
			bool[] bits = new bool[64];
			for ( int i = 0; i < 64; i++ )
			{
				bits[i] = (l & (1UL << i)) != 0UL;
			}
			return bits;
		} ).ToArray();
}

[DebuggerTypeProxy( typeof( BitVectorDebugProxy ) )]
public class BitVector
{
	public const int BitsPerRegion = sizeof( ulong ) * 8;
	private readonly ulong[] mLongArray;

	/// <summary>
	/// Provide the RAW ARRAY ITSELF because <see cref="List{T}"/> doesn't
	/// do that for some reason and yet it's very useful >:(
	/// </summary>
	public ulong[] InternalArray => mLongArray;
	
	public BitVector( int numBits )
	{
		// Round up to nearest multiple of 64
		numBits += BitsPerRegion - 1;
		mLongArray = new ulong[numBits / BitsPerRegion];
	}
	
	public void ClearBits()
	{
		Array.Fill( mLongArray, 0UL );
	}
	
	public void SetBit( int bitIndex, bool value )
	{
		int longIndex = bitIndex / BitsPerRegion;
		int position = bitIndex  % BitsPerRegion;

		if ( value )
		{
			mLongArray[longIndex] |= 1UL << position;
		}
		else
		{
			mLongArray[longIndex] &= ~(1UL << position);
		}
	}

	/// <summary></summary>
	public bool GetBit( int bitIndex )
	{
		int longIndex = bitIndex / BitsPerRegion;
		int position = bitIndex  % BitsPerRegion;

		return (mLongArray[longIndex] & (1UL << position)) != 0;
	}

	/// <summary></summary>
	public int FindLowBit()
	{
		for ( int i = 0; i < mLongArray.Length; i++ )
		{
			ulong flags = mLongArray[i];
			if ( flags == ulong.MaxValue )
			{
				continue;
			}

			if ( flags == 0 )
			{
				return i * BitsPerRegion;
			}

			for ( int bit = 0; bit < BitsPerRegion; bit++ )
			{
				if ( (flags & (1UL << bit)) == 0 )
				{
					return i * BitsPerRegion + bit;
				}
			}
		}

		return -1;
	}

	/// <summary></summary>
	public bool AnyLow()
	{
		for ( int i = 0; i < mLongArray.Length; i++ )
		{
			if ( mLongArray[i] != ulong.MaxValue )
			{
				return true;
			}
		}

		return false;
	}

	/// <summary></summary>
	public bool AnyHigh()
	{
		for ( int i = 0; i < mLongArray.Length; i++ )
		{
			if ( mLongArray[i] != 0UL )
			{
				return true;
			}
		}

		return false;
	}

	/// <summary></summary>
	public bool AnyLowInRegion( int bitIndex )
	{
		int flagIndex = bitIndex / BitsPerRegion;
		return mLongArray[flagIndex] != ulong.MaxValue;
	}

	/// <summary></summary>
	public bool AnyHighInRegion( int bitIndex )
	{
		int flagIndex = bitIndex / BitsPerRegion;
		return mLongArray[flagIndex] != 0;
	}
}
