
using System.Runtime.InteropServices;

namespace Elegy.Extensions
{
	public static class SpanExtensions
	{
		public static ref T GetRef<T>( this Span<T> span )
		{
			return ref MemoryMarshal.GetReference( span );
		}
	}
}
