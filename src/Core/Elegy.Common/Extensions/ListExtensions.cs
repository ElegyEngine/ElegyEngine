// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace Elegy.Common.Extensions
{
	/// <summary>
	/// Elegy-specific <see cref="List{T}"/> extensions.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Sometimes <c>AsSpan</c> is not available for lists, so we can always have this.
		/// </summary>
		public static Span<T> AsSpan<T>( this List<T> list )
		{
			return CollectionsMarshal.AsSpan( list );
		}

		/// <summary>
		/// Interprets a <see cref="List{T}"/> as a <see cref="Span{T}"/> but starting from the <paramref name="start"/>'th element.
		/// </summary>
		public static Span<T> AsSpan<T>( this List<T> list, int start )
		{
			return list.AsSpan().Slice( start );
		}

		/// <summary>
		/// Interprets a <see cref="List{T}"/> as a <see cref="Span{T}"/> but starting from the <paramref name="start"/>'th element,
		/// going up to <paramref name="length"/> elements after that.
		/// </summary>
		public static Span<T> AsSpan<T>( this List<T> list, int start, int length )
		{
			return list.AsSpan().Slice( start, length );
		}

		public static T AddAndGet<T>( this ISet<T> set, T element )
		{
			set.Add( element );
			return element;
		}

		public static T AddAndGet<T>( this IList<T> list, T element )
		{
			list.Add( element );
			return list[list.Count - 1];
		}

		public static T? TryGetAt<T>( this IList<T> list, int elementIndex )
		{
			if ( elementIndex < 0 || elementIndex > list.Count )
			{
				return default( T );
			}

			return list[elementIndex];
		}

		public static bool RemoveAndThen<T>( this ISet<T> set, T element, Action<T> onRemoved )
		{
			if ( set.Remove( element ) )
			{
				onRemoved( element );
				return true;
			}

			return false;
		}

		public static bool TryRemove<T>( this IList<T> list, int elementIndex, Action<T> onRemove )
		{
			T? element = list.TryGetAt( elementIndex );
			if ( element is null )
			{
				return false;
			}

			onRemove( element );
			return true;
		}

		public static bool TryRemove<T>( this IList<T> list, T element, Action<T> onRemove )
		{
			int index = list.IndexOf( element );
			if ( index == -1 )
			{
				return false;
			}

			onRemove( element );
			return true;
		}
	}
}
