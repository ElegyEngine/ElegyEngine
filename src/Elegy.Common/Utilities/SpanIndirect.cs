// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Utilities
{
	/// <summary>
	/// <see cref="Span{T}"/> happens to be a <see langword="ref"/> struct, which doesn't
	/// allow us to use it in regular structs. This is a workaround.
	/// </summary>
	public struct SpanIndirect<T>
	{
		readonly T[] mArray;
		readonly int mStart;
		readonly int mLength;

		/// <summary>
		/// Constructs a <see cref="Span{T}"/> wrapper.
		/// </summary>
		public SpanIndirect( T[] array, int start = 0, int length = -1 )
		{
			mArray = array;
			mStart = start;

			if ( length == -1 )
			{
				length = mArray.Length - mStart;
			}

			mLength = length;
		}

		/// <summary>
		/// Casts an array to a <see cref="SpanIndirect{T}"/>.
		/// </summary>
		public static implicit operator SpanIndirect<T>( T[] array )
		{
			return new SpanIndirect<T>( array );
		}

		/// <summary>
		/// Casts this into <see cref="Span{T}"/>.
		/// </summary>
		public static implicit operator Span<T>( SpanIndirect<T> metaSpan )
		{
			return new Span<T>( metaSpan.mArray, metaSpan.mStart, metaSpan.mLength );
		}

		/// <summary>
		/// Casts this into <see cref="ReadOnlySpan{T}"/>.
		/// </summary>
		public static implicit operator ReadOnlySpan<T>( SpanIndirect<T> metaSpan )
		{
			return new ReadOnlySpan<T>( metaSpan.mArray, metaSpan.mStart, metaSpan.mLength );
		}
	}
}
