// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using Elegy.RenderBackend.Assets;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderSystem.Resources
{
	[StructLayout( LayoutKind.Sequential )]
	internal struct Vector2Byte
	{
		public Vector2Byte( byte x, byte y )
		{
			X = x;
			Y = y;
		}

		public static implicit operator Vector2Byte( Vector2 v )
			=> new(
				(byte)(v.X * 255.0f),
				(byte)(v.Y * 255.0f) );

		public byte X;
		public byte Y;
	}

	[StructLayout( LayoutKind.Sequential )]
	internal struct Vector3Byte
	{
		public Vector3Byte( byte x, byte y, byte z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static implicit operator Vector3Byte( Vector3 v )
			=> new(
				(byte)(v.X * 255.0f),
				(byte)(v.Y * 255.0f),
				(byte)(v.Z * 255.0f) );

		public byte X;
		public byte Y;
		public byte Z;
	}

	public class MaterialParameter
	{
		public MaterialParameter( string name, ShaderDataType type, DeviceBuffer buffer )
		{
			Name = name;
			Type = type;
			Buffer = buffer;
		}

		public MaterialParameter( string name, ShaderDataType type, Texture texture )
		{
			Name = name;
			Type = type;
			Texture = texture;
		}

		public MaterialParameter( string name, Sampler sampler )
		{
			Name = name;
			Type = ShaderDataType.Sampler;
			Sampler = sampler;
		}

		public void SetValue( GraphicsDevice device, int value )
		{
			switch ( Type )
			{
				case ShaderDataType.Byte: device.UpdateBuffer( Buffer, 0, (byte)value ); break;
				case ShaderDataType.Short: device.UpdateBuffer( Buffer, 0, (short)value ); break;
				case ShaderDataType.Int: device.UpdateBuffer( Buffer, 0, value ); break;
			}
		}

		public void SetValue( GraphicsDevice device, float value )
		{
			Debug.Assert( Buffer is not null );
			device.UpdateBuffer( Buffer, 0, value );
		}

		public void SetValue( GraphicsDevice device, bool value )
			=> SetValue( device, value ? 1 : 0 );

		public void SetValue( GraphicsDevice device, Vector2 value )
		{
			Debug.Assert( Buffer is not null );
			switch ( Type )
			{
				case ShaderDataType.Vec2: device.UpdateBuffer( Buffer, 0, value ); break;
				case ShaderDataType.Vec2Byte: device.UpdateBuffer<Vector2Byte>( Buffer, 0, value ); break;
			}
		}

		public void SetValue( GraphicsDevice device, Vector3 value )
		{
			Debug.Assert( Buffer is not null );
			switch ( Type )
			{
				case ShaderDataType.Vec3: device.UpdateBuffer( Buffer, 0, value ); break;
				case ShaderDataType.Vec3Byte: device.UpdateBuffer<Vector3Byte>( Buffer, 0, value ); break;
			}
		}

		public void SetValue( GraphicsDevice device, Vector4 value )
		{
			Debug.Assert( Buffer is not null );
			switch ( Type )
			{
				case ShaderDataType.Vec4: device.UpdateBuffer( Buffer, 0, value ); break;
				case ShaderDataType.Vec4Byte: device.UpdateBuffer( Buffer, 0, new Vector4B( value ) ); break;
			}
		}

		public void SetValue( GraphicsDevice device, Matrix4x4 value )
		{
			Debug.Assert( Buffer is not null );
			device.UpdateBuffer( Buffer, 0, value );
		}

		public void SetBufferValue<T>( GraphicsDevice device, T bufferValue ) where T : unmanaged
		{
			Debug.Assert( Buffer is not null );
			device.UpdateBuffer( Buffer, 0, bufferValue );
		}

		public string Name { get; private set; } = string.Empty;
		public ShaderDataType Type { get; private set; } = ShaderDataType.Buffer;
		public DeviceBuffer? Buffer { get; private set; } = null;
		public Texture? Texture { get; set; } = null;
		public Sampler? Sampler { get; set; } = null;
	}
}
