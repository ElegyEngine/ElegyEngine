
using System.Runtime.InteropServices;

namespace Elegy.Extensions
{
	public static class ListExtensions
	{
		public static Span<T> AsSpan<T>( this List<T> list )
		{
			return CollectionsMarshal.AsSpan( list );
		}

		public static Span<T> AsSpan<T>( this List<T> list, int start )
		{
			return list.AsSpan().Slice( start );
		}

		public static Span<T> AsSpan<T>( this List<T> list, int start, int length )
		{
			return list.AsSpan().Slice( start, length );
		}
	}
}
