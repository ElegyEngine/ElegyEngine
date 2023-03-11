// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Text;

namespace Elegy.Utilities.Interfaces
{
	/// <summary>
	/// Byte buffer, a container that you can use to conveniently
	/// read from and write to binary blobs of data.
	/// </summary>
	public interface IByteBuffer
	{
		public int Position { get; }
		public IReadOnlyList<byte> Data { get; }
		public ReadOnlySpan<byte> DataSpan { get; }

		public void Advance( int bytes );
		public void ResetPosition();
		public void ResetData();

		/// <summary>
		/// Writes generic data into the buffer.
		/// </summary>
		/// <typeparam name="T">The type of data that gets written.</typeparam>
		public void Write<T>( T value ) where T : unmanaged;

		/// <summary>
		/// Reads generic data from the buffer.
		/// </summary>
		/// <typeparam name="T">The type of data that gets read.</typeparam>
		public T Read<T>() where T : unmanaged;

		public void WriteU8( byte value ) => Write( value );
		public void WriteBool( bool value ) => WriteU8( (byte)(value ? 1 : 0) );
		public void WriteI16( short value );
		public void WriteU16( ushort value );
		public void WriteI32( int value );
		public void WriteU32( uint value );
		public void WriteI64( long value );
		public void WriteU64( ulong value );
		public void WriteF16( Half value );
		public void WriteF32( float value );
		public void WriteF64( double value );


		public byte ReadU8() => Read<byte>();
		public bool ReadBool() => ReadU8() > 0;
		public short ReadI16();
		public ushort ReadU16();
		public int ReadI32();
		public uint ReadU32();
		public long ReadI64();
		public ulong ReadU64();
		public Half ReadF16();
		public float ReadF32();
		public double ReadF64();

		public void WriteObject<T>( T value ) where T : IByteSerialisable;
		public T ReadObject<T>() where T : IByteSerialisable, new();

		/// <summary>
		/// Writes a string in the format: number of characters, raw string contents.
		/// </summary>
		/// <param name="stringEncoding">What type of string encoding to use (ASCII, UTF-8).</param>
		public void WriteString( string value, Encoding stringEncoding );

		/// <summary>
		/// Writes an ASCII-encoded string.
		/// </summary>
		public void WriteStringAscii( string value );

		/// <summary>
		/// Writes a UTF-8-encoded string.
		/// </summary>
		public void WriteStringUtf8( string value );

		/// <summary>
		/// Reads a string from the buffer.
		/// </summary>
		/// <param name="stringEncoding">What type of encoding to use.</param>
		/// <exception cref="IndexOutOfRangeException"/>
		public string ReadString( Encoding stringEncoding );

		/// <summary>
		/// Reads an ASCII-encoded string from the buffer.
		/// </summary>
		public string ReadStringAscii();

		/// <summary>
		/// Reads a UTF-8-encoded string from the buffer.
		/// </summary>
		public string ReadStringUtf8();
	}
}
