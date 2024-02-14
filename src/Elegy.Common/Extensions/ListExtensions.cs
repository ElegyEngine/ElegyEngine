
using System.Runtime.InteropServices;

namespace Elegy.Extensions
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
	}
}
