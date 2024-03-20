// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces;
using Elegy.Engine.Interfaces.Rendering;
using Elegy.RenderBackend.Assets;

using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

using ShaderDataType = Elegy.RenderBackend.Assets.ShaderDataType;

namespace Elegy.RenderStandard;

[StructLayout(LayoutKind.Sequential)]
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

[StructLayout( LayoutKind.Sequential )]
internal struct Vector4Byte
{
	public Vector4Byte( byte x, byte y, byte z, byte w )
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public static implicit operator Vector4Byte( Vector4 v )
		=> new(
			(byte)(v.X * 255.0f),
			(byte)(v.Y * 255.0f),
			(byte)(v.Z * 255.0f),
			(byte)(v.W * 255.0f) );

	public byte X;
	public byte Y;
	public byte Z;
	public byte W;
}

public class RenderMaterialParameter
{
	public RenderMaterialParameter( DeviceBuffer buffer )
	{
		Buffer = buffer;
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
		device.UpdateBuffer( Buffer, 0, value );
	}

	public void SetValue( GraphicsDevice device, bool value )
		=> SetValue( device, value ? 1 : 0 );

	public void SetValue( GraphicsDevice device, Vector2 value )
	{
		switch ( Type )
		{
			case ShaderDataType.Vec2: device.UpdateBuffer( Buffer, 0, value ); break;
			case ShaderDataType.Vec2Byte: device.UpdateBuffer<Vector2Byte>( Buffer, 0, value ); break;
		}
	}

	public void SetValue( GraphicsDevice device, Vector3 value )
	{
		switch ( Type )
		{
			case ShaderDataType.Vec3: device.UpdateBuffer( Buffer, 0, value ); break;
			case ShaderDataType.Vec3Byte: device.UpdateBuffer<Vector3Byte>( Buffer, 0, value ); break;
		}
	}

	public void SetValue( GraphicsDevice device, Vector4 value )
	{
		switch ( Type )
		{
			case ShaderDataType.Vec4: device.UpdateBuffer( Buffer, 0, value ); break;
			case ShaderDataType.Vec4Byte: device.UpdateBuffer<Vector4Byte>( Buffer, 0, value ); break;
		}
	}

	public void SetValue( GraphicsDevice device, Matrix4x4 value )
	{
		device.UpdateBuffer( Buffer, 0, value );
	}

	public void SetBufferValue<T>( GraphicsDevice device, T bufferValue ) where T: unmanaged
	{
		device.UpdateBuffer( Buffer, 0, bufferValue );
	}

	public string Name { get; set; } = string.Empty;
	public int SetId { get; set; } = 0;
	public int BindingId { get; set; } = 0;
	public ShaderDataType Type { get; set; } = ShaderDataType.Buffer;
	public DeviceBuffer Buffer { get; set; }
}

public class RenderMaterial : IMaterial
{
	public RenderMaterial( GraphicsDevice device, MaterialTemplate materialTemplate )
	{
		mDevice = device;
		
	}

	private GraphicsDevice mDevice;
	public List<RenderMaterialParameter> Parameters { get; set; } = new();

	public string[] GetParameterNames()
	{
		return Array.Empty<string>();
	}

	public int GetParameterIndex( string name )
	{
		for ( int i = 0; i < Parameters.Count; i++ )
		{
			if ( Parameters[i].Name == name )
			{
				return i;
			}
		}

		return -1;
	}

	public void SetParameter( int id, int value )
	{
		if ( ValidateIntention( id, ShaderDataType.Int, ShaderDataType.Short, ShaderDataType.Byte ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetParameter( int id, float value )
	{
		if ( ValidateIntention( id, ShaderDataType.Float ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetParameter( int id, bool value )
	{
		if ( ValidateIntention( id, ShaderDataType.Byte ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetParameter( int id, Vector2 value )
	{
		if ( ValidateIntention( id, ShaderDataType.Vec2, ShaderDataType.Vec2Byte ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetParameter( int id, Vector3 value )
	{
		if ( ValidateIntention( id, ShaderDataType.Vec3, ShaderDataType.Vec3Byte ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetParameter( int id, Vector4 value )
	{
		if ( ValidateIntention( id, ShaderDataType.Vec4, ShaderDataType.Vec4Byte ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetParameter( int id, Matrix4x4 value )
	{
		if ( ValidateIntention( id, ShaderDataType.Mat44 ) )
		{
			Parameters[id].SetValue( mDevice, value );
		}
	}

	public void SetBufferParameter<T>( int id, T bufferValue ) where T: unmanaged
	{
		if ( ValidateIntention( id, ShaderDataType.Buffer, ShaderDataType.BufferRW ) )
		{
			Parameters[id].SetBufferValue( mDevice, bufferValue );
		}
	}

	private bool ValidateIntention( int id, params ShaderDataType[] types )
	{
		if ( id >= Parameters.Count )
		{
			Console.Error( "RenderMaterial", $"Index is out of range ({id})" );
			return false;
		}

		if ( !types.Contains( Parameters[id].Type ) )
		{
			Console.Error( "RenderMaterial", $"Type mismatch, parameter is {Parameters[id].Type} but value is {types[0]} ({Parameters[id].Name})" );
			return false;
		}

		return true;
	}
}

public partial class RenderStandard : IRenderFrontend
{
	public IMaterial CreateMaterial( MaterialDefinition materialDefinition )
	{
		throw new NotImplementedException();
	}


	public bool FreeMaterial( IMaterial material )
	{
		throw new NotImplementedException();
	}
}
