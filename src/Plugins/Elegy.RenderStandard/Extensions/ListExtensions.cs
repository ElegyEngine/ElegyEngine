// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderStandard.Extensions
{
	internal static class ListExtensions
	{
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
				return default(T);
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
