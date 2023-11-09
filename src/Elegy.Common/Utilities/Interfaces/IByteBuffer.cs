// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Text;

namespace Elegy.Utilities.Interfaces
{
	public enum StringLength
	{
		/// <summary>
		/// Up to 255 characters.
		/// </summary>
		Short,

		/// <summary>
		/// Up to 65'535 characters.
		/// </summary>
		Medium,

		/// <summary>
		/// Up to approximately 2.147 billion characters.
		/// </summary>
		Long
	}

	/// <summary>
	/// Byte buffer, a container that you can use to conveniently
	/// read from and write to binary blobs of data.
	/// </summary>
	public interface IByteBuffer
	{
		/// <summary>
		/// Cursor position.
		/// </summary>
		public int Position { get; }
		/// <summary>
		/// Buffer data.
		/// </summary>
		public IReadOnlyList<byte> Data { get; }
		/// <summary>
		/// Span version of <see cref="Data"/>.
		/// </summary>
		public ReadOnlySpan<byte> DataSpan { get; }

		/// <summary>
		/// Advances the cursor <paramref name="bytes"/> number of bytes.
		/// </summary>
		public void Advance( int bytes );
		/// <summary>
		/// Resets the cursor to the beginning of the buffer.
		/// </summary>
		public void ResetPosition();
		/// <summary>
		/// Clears the data buffer.
		/// </summary>
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

		/// <summary>
		/// Writes an unsigned byte value and advances 1 byte.
		/// </summary>
		public void WriteU8( byte value );
		/// <summary>
		/// Writes a boolean value and advances 1 byte.
		/// </summary>
		public void WriteBool( bool value );
		/// <summary>
		/// Writes an ASCII character value and advances 1 byte.
		/// </summary>
		public void WriteChar( char value );
		/// <summary>
		/// Writes an enum value and advances 1 byte.
		/// </summary>
		public void WriteEnum<T>( T value ) where T : struct, Enum, IConvertible;
		/// <summary>
		/// Writes a signed short value and advances 2 bytes.
		/// </summary>
		public void WriteI16( short value );
		/// <summary>
		/// Writes an unsigned short value and advances 2 bytes.
		/// </summary>
		public void WriteU16( ushort value );
		/// <summary>
		/// Writes a signed integer value and advances 4 bytes.
		/// </summary>
		public void WriteI32( int value );
		/// <summary>
		/// Writes an unsigned integer value and advances 4 bytes.
		/// </summary>
		public void WriteU32( uint value );
		/// <summary>
		/// Writes a signed long value and advances 8 bytes.
		/// </summary>
		public void WriteI64( long value );
		/// <summary>
		/// Writes an unsigned long value and advances 8 bytes.
		/// </summary>
		public void WriteU64( ulong value );
		/// <summary>
		/// Writes a half-precision float value and advances 2 bytes.
		/// </summary>
		public void WriteF16( Half value );
		/// <summary>
		/// Writes a single-precision float value and advances 4 bytes.
		/// </summary>
		public void WriteF32( float value );
		/// <summary>
		/// Writes a double-precision float value and advances 8 bytes.
		/// </summary>
		public void WriteF64( double value );

		/// <summary>
		/// Reads an unsigned byte value and advances 1 byte.
		/// </summary>
		public byte ReadU8();
		/// <summary>
		/// Reads a boolean value and advances 1 byte.
		/// </summary>
		public bool ReadBool();
		/// <summary>
		/// Reads a boolean value and advances 1 byte.
		/// </summary>
		public char ReadChar();
		/// <summary>
		/// Reads an ASCII character value and advances 1 byte.
		/// </summary>
		public T ReadEnum<T>() where T : struct, Enum;
		/// <summary>
		/// Reads a signed short value and advances 2 bytes.
		/// </summary>
		public short ReadI16();
		/// <summary>
		/// Reads an unsigned short value and advances 2 bytes.
		/// </summary>
		public ushort ReadU16();
		/// <summary>
		/// Reads a signed integer value and advances 4 bytes.
		/// </summary>
		public int ReadI32();
		/// <summary>
		/// Reads an unsigned integer value and advances 4 bytes.
		/// </summary>
		public uint ReadU32();
		/// <summary>
		/// Reads a signed long value and advances 8 bytes.
		/// </summary>
		public long ReadI64();
		/// <summary>
		/// Reads an unsigned long value and advances 8 bytes.
		/// </summary>
		public ulong ReadU64();
		/// <summary>
		/// Reads a half-precision float value and advances 2 bytes.
		/// </summary>
		public Half ReadF16();
		/// <summary>
		/// Reads a single-precision float value and advances 4 bytes.
		/// </summary>
		public float ReadF32();
		/// <summary>
		/// Reads a double-precision float value and advances 8 bytes.
		/// </summary>
		public double ReadF64();

		/// <summary>
		/// Writes/serialises an object that implements <see cref="IByteSerialisable"/>.
		/// </summary>
		public void WriteObject<T>( T value ) where T : IByteSerialisable;
		/// <summary>
		/// Reads/deserialises an object that implements <see cref="IByteSerialisable"/>.
		/// </summary>
		public T ReadObject<T>() where T : IByteSerialisable, new();

		/// <summary>
		/// Writes a string in the format: number of characters, raw string contents.
		/// </summary>
		/// <param name="stringEncoding">What type of string encoding to use (ASCII, UTF-8).</param>
		public void WriteString( string value, Encoding stringEncoding, StringLength stringLength );

		/// <summary>
		/// Writes an ASCII-encoded string.
		/// </summary>
		public void WriteStringAscii( string value, StringLength stringLength );

		/// <summary>
		/// Writes a UTF-8-encoded string.
		/// </summary>
		public void WriteStringUtf8( string value, StringLength stringLength );

		/// <summary>
		/// Reads a string from the buffer.
		/// </summary>
		/// <param name="stringEncoding">What type of encoding to use.</param>
		/// <exception cref="IndexOutOfRangeException"/>
		public string ReadString( Encoding stringEncoding, StringLength stringLength );

		/// <summary>
		/// Reads an ASCII-encoded string from the buffer.
		/// </summary>
		public string ReadStringAscii( StringLength stringLength );

		/// <summary>
		/// Reads a UTF-8-encoded string from the buffer.
		/// </summary>
		public string ReadStringUtf8( StringLength stringLength );
	}
}
