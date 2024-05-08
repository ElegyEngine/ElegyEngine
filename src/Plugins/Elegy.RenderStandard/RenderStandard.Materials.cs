﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Maths;
using Elegy.AssetSystem.API;
using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;
using Elegy.RenderBackend.Assets;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

using ShaderDataType = Elegy.RenderBackend.Assets.ShaderDataType;
using Collections.Pooled;

using Material = Elegy.AssetSystem.Resources.Material;
using Elegy.RenderStandard.Extensions;

namespace Elegy.RenderStandard;

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

public class RenderTexture : ITexture, IDisposable
{
	private GraphicsDevice mDevice;

	public RenderTexture( GraphicsDevice device, in TextureMetadata data, in Span<byte> bytes )
	{
		mDevice = device;
		DeviceTexture = device.ResourceFactory.CreateTexture( new()
		{
			Width = Math.Max( 1, data.Width ),
			Height = Math.Max( 1, data.Height ),
			Depth = Math.Max( 1, data.Depth ),

			ArrayLayers = 1,
			// TODO: mipmapping
			MipLevels = 1,

			// TODO: Make this more complete and rigorous
			Format = data.Compression switch
			{
				TextureCompression.Dxt1 => (data.Components, data.Srgb) switch
				{
					( 3, false ) => PixelFormat.BC1_Rgb_UNorm,
					( 3, true ) => PixelFormat.BC1_Rgb_UNorm_SRgb,
					( 4, false ) => PixelFormat.BC1_Rgba_UNorm,
					( 4, true ) => PixelFormat.BC1_Rgba_UNorm_SRgb,
					_ => throw new NotSupportedException()
				},

				TextureCompression.Dxt5 => data.Srgb switch
				{
					true => PixelFormat.BC3_UNorm_SRgb,
					false => PixelFormat.BC3_UNorm
				},

				TextureCompression.None => (data.BytesPerPixel, data.Components, data.Srgb, data.Float) switch
				{
					( 1, 4, false, false ) => PixelFormat.B8_G8_R8_A8_UNorm,
					( 1, 4, true, false ) => PixelFormat.B8_G8_R8_A8_UNorm_SRgb,

					( 4, 1, false, true ) => PixelFormat.R32_Float,
					( 4, 2, false, true ) => PixelFormat.R32_G32_Float,
					( 4, 4, false, true ) => PixelFormat.R32_G32_B32_A32_Float,

					( 4, 1, false, false ) => PixelFormat.R32_UInt,
					( 4, 2, false, false ) => PixelFormat.R32_G32_UInt,
					( 4, 4, false, false ) => PixelFormat.R32_G32_B32_A32_UInt,

					_ => throw new NotSupportedException()
				},

				_ => throw new NotImplementedException()
			},

			SampleCount = TextureSampleCount.Count1,
			Type = data.Is1D switch
			{
				true => TextureType.Texture1D,
				false => data.Is2D switch
				{
					true => TextureType.Texture2D,
					false => TextureType.Texture3D
				}
			},
			Usage = TextureUsage.Sampled
		} );

		UpdatePixels( bytes );
	}

	public int Width => (int)DeviceTexture.Width;
	public int Height => (int)DeviceTexture.Height;
	public int Depth => (int)DeviceTexture.Depth;

	public Texture DeviceTexture { get; set; }

	public Span<byte> ReadPixels()
	{
		return Array.Empty<byte>();
	}

	public void UpdatePixels( Span<byte> newPixels )
	{
		mDevice.UpdateTexture( DeviceTexture, newPixels, 0, 0, 0, DeviceTexture.Width, DeviceTexture.Height, DeviceTexture.Depth, 0, 0 );
	}

	public void Dispose()
	{
		DeviceTexture.Dispose();
	}
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
		=> device.UpdateBuffer( Buffer, 0, value );

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
			case ShaderDataType.Vec4Byte: device.UpdateBuffer( Buffer, 0, new Vector4B( value ) ); break;
		}
	}

	public void SetValue( GraphicsDevice device, Matrix4x4 value )
	{
		device.UpdateBuffer( Buffer, 0, value );
	}

	public void SetBufferValue<T>( GraphicsDevice device, T bufferValue ) where T : unmanaged
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
	public RenderMaterial( GraphicsDevice device, MaterialDefinition materialDefinition )
	{
		mDevice = device;
		MaterialDefinition = materialDefinition;
	}

	private GraphicsDevice mDevice;
	public List<RenderMaterialParameter> Parameters { get; set; } = new();
	public MaterialDefinition MaterialDefinition { get; private set; }

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

	public void SetBufferParameter<T>( int id, T bufferValue ) where T : unmanaged
	{
		if ( ValidateIntention( id, ShaderDataType.Buffer, ShaderDataType.BufferRW ) )
		{
			Parameters[id].SetBufferValue( mDevice, bufferValue );
		}
	}

	public void SetTexture( int id, ITexture value )
	{
		throw new NotImplementedException();
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
	// TODO: dynamic capacity configuration
	PooledSet<RenderMaterial> mMaterialSet = new( 8192 );
	PooledSet<RenderTexture> mTextureSet = new( 16384 );

	public IMaterial? CreateMaterial( MaterialDefinition materialDefinition )
		=> mMaterialSet.AddAndGet( new( mDevice, materialDefinition ) );

	public RenderMaterial GetMaterial( string name )
	{
		var material = Assets.LoadMaterial( name );

		Debug.Assert( material is not null );
		Debug.Assert( material.RenderMaterial is not null );

		return (RenderMaterial)material.RenderMaterial;
	}

	public bool FreeMaterial( IMaterial material )
		=> mMaterialSet.Remove( (RenderMaterial)material );

	public ITexture? CreateTexture( TextureMetadata metadata, Span<byte> data )
		=> mTextureSet.AddAndGet( new( mDevice, metadata, data ) );

	public bool FreeTexture( ITexture texture )
		=> mTextureSet.RemoveAndThen( (RenderTexture)texture, ( texture ) =>
		{
			texture.Dispose();
		} );
}
