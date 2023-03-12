// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Elegy.Utilities.Interfaces;

namespace Elegy.Utilities
{
    public class ByteBuffer : IByteBuffer
	{
		public int Position { get; private set; } = 0;
		public IReadOnlyList<byte> Data => mData;
		public ReadOnlySpan<byte> DataSpan => mData.AsSpan();

		private byte[] mData;

		public ByteBuffer( byte[] data )
		{
			mData = data;
		}

		public ByteBuffer( int size )
		{
			mData = new byte[size];
		}

		public void Advance( int bytes )
		{
			Position += bytes;

			if ( Position > Data.Count )
			{
				throw new IndexOutOfRangeException( "Position > Data.Count" );
			}
			else if ( Position < 0 )
			{
				throw new IndexOutOfRangeException( "Position < 0" );
			}
		}

		public void ResetPosition()
		{
			Position = 0;
		}

		public void ResetData()
		{
			Unsafe.InitBlockUnaligned( 
				ref MemoryMarshal.GetReference( DataSpan ), 0, (uint)Data.Count );
		}

		#region Generic writing and reading
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		Span<byte> GetCurrentSpan()
		{
			return mData.AsSpan( Position );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Write<T>( T value ) where T : unmanaged
		{
			Unsafe.WriteUnaligned( ref GetCurrentSpan().GetRef(), value );
			Advance( Marshal.SizeOf<T>() );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public T Read<T>() where T : unmanaged
		{
			T value = Unsafe.ReadUnaligned<T>( ref GetCurrentSpan().GetRef() );
			Advance( Marshal.SizeOf<T>() );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteU8( byte value )
			=> Write( value );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteBool( bool value )
			=> Write( (byte)(value ? 1 : 0) );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteChar( char value )
			=> Write( (byte)value );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteEnum<T>( T value ) where T : struct, Enum, IConvertible
					=> Write( value.ToByte( CultureInfo.InvariantCulture ) );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteI16( short value )
		{
			BinaryPrimitives.WriteInt16LittleEndian( GetCurrentSpan(), value );
			Advance( 2 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteU16( ushort value )
		{
			BinaryPrimitives.WriteUInt16LittleEndian( GetCurrentSpan(), value );
			Advance( 2 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteI32( int value )
		{
			BinaryPrimitives.WriteInt32LittleEndian( GetCurrentSpan(), value );
			Advance( 4 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteU32( uint value )
		{
			BinaryPrimitives.WriteUInt32LittleEndian( GetCurrentSpan(), value );
			Advance( 4 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteI64( long value )
		{
			BinaryPrimitives.WriteInt64LittleEndian( GetCurrentSpan(), value );
			Advance( 8 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteU64( ulong value )
		{
			BinaryPrimitives.WriteUInt64LittleEndian( GetCurrentSpan(), value );
			Advance( 8 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteF16( Half value )
		{
			BinaryPrimitives.WriteHalfLittleEndian( GetCurrentSpan(), value );
			Advance( 2 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteF32( float value )
		{
			BinaryPrimitives.WriteSingleLittleEndian( GetCurrentSpan(), value );
			Advance( 4 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteF64( double value )
		{
			BinaryPrimitives.WriteDoubleLittleEndian( GetCurrentSpan(), value );
			Advance( 8 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public byte ReadU8()
			=> Read<byte>();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool ReadBool()
			=> ReadU8() > 0;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public char ReadChar()
			=> (char)ReadU8();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public T ReadEnum<T>() where T : struct, Enum
			=> (T)Enum.ToObject( typeof( T ), ReadU8() );

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public short ReadI16()
		{
			var value = BinaryPrimitives.ReadInt16LittleEndian( GetCurrentSpan() );
			Advance( 2 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ushort ReadU16()
		{
			var value = BinaryPrimitives.ReadUInt16LittleEndian( GetCurrentSpan() );
			Advance( 2 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int ReadI32()
		{
			var value = BinaryPrimitives.ReadInt32LittleEndian( GetCurrentSpan() );
			Advance( 4 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public uint ReadU32()
		{
			var value = BinaryPrimitives.ReadUInt32LittleEndian( GetCurrentSpan() );
			Advance( 4 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public long ReadI64()
		{
			var value = BinaryPrimitives.ReadInt64LittleEndian( GetCurrentSpan() );
			Advance( 8 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ulong ReadU64()
		{
			var value = BinaryPrimitives.ReadUInt64LittleEndian( GetCurrentSpan() );
			Advance( 8 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Half ReadF16()
		{
			var value = BinaryPrimitives.ReadHalfLittleEndian( GetCurrentSpan() );
			Advance( 2 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float ReadF32()
		{
			var value = BinaryPrimitives.ReadSingleLittleEndian( GetCurrentSpan() );
			Advance( 4 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public double ReadF64()
		{
			var value = BinaryPrimitives.ReadDoubleLittleEndian( GetCurrentSpan() );
			Advance( 8 );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteObject<T>( T value ) where T : IByteSerialisable
		{
			value.Serialise( this );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public T ReadObject<T>() where T : IByteSerialisable, new()
		{
			T value = new();
			value.Deserialise( this );
			return value;
		}

		#endregion

		#region String writing

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteString( string value, Encoding stringEncoding, StringLength stringLength )
		{
			byte[] characterBuffer = Array.Empty<byte>();
			int bufferLength = 0;

			if ( !string.IsNullOrEmpty( value ) )
			{
				characterBuffer = stringEncoding.GetBytes( value );
				bufferLength = characterBuffer.Length;
			}

			switch ( stringLength )
			{
				case StringLength.Short:
					bufferLength = Math.Clamp( (byte)characterBuffer.Length, byte.MinValue, byte.MaxValue );
					Write( (byte)bufferLength );
					break;

				case StringLength.Medium:
					bufferLength = Math.Clamp( (ushort)characterBuffer.Length, ushort.MinValue, ushort.MaxValue );
					WriteU16( (ushort)bufferLength );
					break;

				case StringLength.Long:
					WriteI32( characterBuffer.Length );
					break;
			}

			if ( bufferLength > 0 )
			{
				Unsafe.CopyBlockUnaligned( ref mData[Position], ref characterBuffer[0], (uint)bufferLength );
			}
			
			Advance( bufferLength );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteStringAscii( string value, StringLength stringLength )
		{
			WriteString( value, Encoding.ASCII, stringLength );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void WriteStringUtf8( string value, StringLength stringLength )
		{
			WriteString( value, Encoding.UTF8, stringLength );
		}

		#endregion

		#region String reading

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string ReadString( Encoding stringEncoding, StringLength stringLength )
		{
			int length = stringLength switch
			{
				StringLength.Short => Read<byte>(),
				StringLength.Medium => ReadU16(),
				StringLength.Long => ReadI32(),
				_ => 0
			};

			if ( length < 0 )
			{
				throw new IndexOutOfRangeException( "Length < 0" );
			}
			else if ( (Position + length) > Data.Count )
			{
				throw new IndexOutOfRangeException( "Position + length > Data.Length" );
			}

			// Sometimes we can have zero-length strings in there
			string value = string.Empty;
			if ( length > 0 )
			{
				stringEncoding.GetString( mData.AsSpan( Position, length ) );
			}

			Advance( length );
			return value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string ReadStringAscii( StringLength stringLength )
		{
			return ReadString( Encoding.ASCII, stringLength );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string ReadStringUtf8( StringLength stringLength )
		{
			return ReadString( Encoding.UTF8, stringLength );
		}

		#endregion
	}
}
