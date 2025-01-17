using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace XrResearch;

public static unsafe class XrUnsafeExtensions
{
	public static T* Deref<T>( this IntPtr self )
		where T : unmanaged
	{
		return (T*)self;
	}

	public static T** DerefDouble<T>( this IntPtr self )
		where T : unmanaged
	{
		return (T**)self;
	}

	public static T* Deref<T>( this List<T> self )
		where T : unmanaged
	{
		return self.AsSpan().AsIntPtr().Deref<T>();
	}

	public static T** DerefDouble<T>( this List<T> self )
		where T : unmanaged
	{
		return self.AsSpan().AsIntPtr().DerefDouble<T>();
	}
	
	public static T* Deref<T>( this T[] self )
		where T : unmanaged
	{
		return self.AsSpan().AsIntPtr().Deref<T>();
	}

	public static T** DerefDouble<T>( this T[] self )
		where T : unmanaged
	{
		return self.AsSpan().AsIntPtr().DerefDouble<T>();
	}

	public static T* Deref<T>( this List<IntPtr> self )
		where T : unmanaged
	{
		return self.AsSpan().AsIntPtr().Deref<T>();
	}

	public static T** DerefDouble<T>( this List<IntPtr> self )
		where T : unmanaged
	{
		return self.AsSpan().AsIntPtr().DerefDouble<T>();
	}

	public static Span<T> AsSpan<T>( this List<T> list )
		=> CollectionsMarshal.AsSpan( list );

	public static IntPtr AsIntPtr<T>( this Span<T> self )
		=> self.Length == 0 ? IntPtr.Zero : (IntPtr)Unsafe.AsPointer( ref self[0] );

	public static void CStringCopyTo( this string source, byte* destination, int length )
	{
		source.CStringCopyTo( destination, (uint)length );
	}

	public static void CStringCopyTo( this string source, byte* destination, uint length )
	{
		int stringEnd = Math.Min( source.Length, (int)length - 1 );

		Span<byte> bytes = new( destination, stringEnd );
		Encoding.ASCII.GetBytes( source, bytes );
		destination[stringEnd] = (byte)'\0';
	}

	public static int CStringLength( byte* bytes )
	{
		for ( int i = 0; i < 1024; i++ )
		{
			if ( bytes[i] == '\0' )
			{
				return i;
			}
		}

		return -1;
	}

	public static int CStringLength( this ReadOnlySpan<byte> bytes )
	{
		for ( int i = 0; i < bytes.Length; i++ )
		{
			if ( bytes[i] == '\0' )
			{
				return i - 1;
			}
		}

		return 0;
	}

	public static bool CStringEquals( this string source, byte* other )
	{
		var span = new ReadOnlySpan<byte>( other, CStringLength( other ) );
		if ( span.Length != source.Length )
		{
			return false;
		}

		for ( int i = 0; i < span.Length; i++ )
		{
			if ( span[i] != (byte)source[i] )
			{
				return false;
			}
		}

		return true;
	}

	public static ReadOnlySpan<byte> TrimNull( this ReadOnlySpan<byte> bytes )
	{
		for ( int i = 0; i < bytes.Length; i++ )
		{
			if ( bytes[i] == '\0' )
			{
				return bytes.Slice( 0, i - 1 );
			}
		}

		return bytes;
	}

	public static string CStringToString( byte* bytes, int maxLength = -1 )
	{
		if ( maxLength == -1 )
		{
			maxLength = CStringLength( bytes );
		}

		ReadOnlySpan<byte> span = new( bytes, maxLength );
		return Encoding.ASCII.GetString( span.TrimNull() );
	}
}
