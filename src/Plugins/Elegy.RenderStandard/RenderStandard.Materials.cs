// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Maths;
using Elegy.ConsoleSystem;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderBackend.Templating;
using Elegy.RenderStandard.Extensions;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

using ShaderDataType = Elegy.RenderBackend.Assets.ShaderDataType;
using Collections.Pooled;

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
					( 3, true )  => PixelFormat.BC1_Rgb_UNorm_SRgb,
					( 4, false ) => PixelFormat.BC1_Rgba_UNorm,
					( 4, true )  => PixelFormat.BC1_Rgba_UNorm_SRgb,
					_ => throw new NotSupportedException()
				},

				TextureCompression.Dxt5 => data.Srgb switch
				{
					true => PixelFormat.BC3_UNorm_SRgb,
					false => PixelFormat.BC3_UNorm
				},

				TextureCompression.None => (data.BytesPerPixel, data.Components, data.Srgb, data.Float) switch
				{
					( 1, 4, false, false ) => PixelFormat.R8_G8_B8_A8_UNorm,
					( 1, 4, true, false ) => PixelFormat.R8_G8_B8_A8_UNorm_SRgb,

					( 2, 1, false, true ) => PixelFormat.R16_Float,
					( 2, 2, false, true ) => PixelFormat.R16_G16_Float,
					( 2, 4, false, true ) => PixelFormat.R16_G16_B16_A16_Float,

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
	public RenderMaterialParameter( string name, ShaderDataType type, DeviceBuffer buffer )
	{
		Name = name;
		Type = type;
		Buffer = buffer;
	}

	public RenderMaterialParameter( string name, ShaderDataType type, Texture texture )
	{
		Name = name;
		Type = type;
		Texture = texture;
	}

	public RenderMaterialParameter( string name, Sampler sampler )
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
	public Sampler? Sampler { get; private set; } = null;
}

public class RenderMaterial : IMaterial
{
	private static TaggedLogger mLogger = new( "RenderMaterial" );

	public RenderMaterial( GraphicsDevice device, MaterialDefinition definition, MaterialTemplate materialTemplate, int[] builtinLayoutIds )
	{
		mDevice = device;
		Definition = definition;
		Template = materialTemplate;

		Parameters.EnsureCapacity( materialTemplate.ShaderTemplate.Parameters.Count - builtinLayoutIds.Length );

		// From the shader parametres, we will look up the given material def's parametres
		// And then generate buffers for em
		foreach ( var parametre in materialTemplate.ShaderTemplate.Parameters.AsSpan() )
		{
			if ( IsParameterBuiltin( parametre, builtinLayoutIds ) )
			{
				continue;
			}

			if ( parametre.Type == ShaderDataType.Buffer || parametre.Type == ShaderDataType.BufferRW )
			{
				mLogger.Warning( $"Parametre '{parametre.Name}' is an unknown buffer type (mat: {definition.Name})" );
				continue;
			}

			string? value = definition.Parameters.GetValueOrDefault( parametre.Name );

			Parameters.Add( parametre.Type.IsTexture() switch
			{
				true => new( parametre.Name, parametre.Type, CreateTextureForMaterialParameter( device, parametre, value ) ),
				false => (parametre.Type == ShaderDataType.Sampler) switch
				{
					// It's NearestSampler for now, will parse them laters
					true => new( parametre.Name, RenderStandard.NearestSampler ),
					false => new( parametre.Name, parametre.Type, CreateBufferForMaterialParameter( device, parametre, value ) )
				}
			} );
		}

		ResourceSets.EnsureCapacity( materialTemplate.ShaderVariants.Count );

		// Generate resource sets from material-level
		// parametres and shader variants
		foreach ( var variantAsset in materialTemplate.ShaderTemplate.ShaderVariants )
		{
			var variant = materialTemplate.ShaderVariants[variantAsset.ShaderDefine];
			ResourceSets.Add( variantAsset.ShaderDefine, GenerateResourceSetsForVariant( device, variant, variantAsset, builtinLayoutIds ) );
		}
	}

	private ResourceSet[] GenerateResourceSetsForVariant( GraphicsDevice device, ShaderVariant variant, RenderBackend.Assets.ShaderTemplateEntry item, int[] builtinLayoutIds )
	{
		// This loop generates a ResourceSet
		int setId = 0;
		ResourceSetDescription[] resourceSetDescriptions = new ResourceSetDescription[item.ResourceLayouts.Count - builtinLayoutIds.Length];
		for ( int i = 0; i < item.ResourceLayouts.Count; i++ )
		{
			if ( builtinLayoutIds.Contains( item.ResourceLayouts[i].Set ) )
			{
				continue;
			}

			// This loop generates the elements for a ResourceSet
			BindableResource[] bindableResources = new BindableResource[item.ResourceLayouts[i].Elements.Count];
			ResourceLayout layout = variant.Layouts[i];
			for ( int e = 0; e < item.ResourceLayouts[i].Elements.Count; e++ )
			{
				var element = item.ResourceLayouts[i].Elements[e];
				int index = GetParameterIndex( element.Name );
				Debug.Assert( index >= 0 );

				if ( element.Type.IsTexture() )
				{
					bindableResources[e] = Parameters[index].Texture;
				}
				else if ( element.Type == ShaderDataType.Sampler )
				{
					bindableResources[e] = Parameters[index].Sampler;
				}
				else
				{
					bindableResources[e] = Parameters[index].Buffer;
				}
			}

			resourceSetDescriptions[setId++] = new()
			{
				Layout = variant.Layouts[i],
				BoundResources = bindableResources
			};
		}

		return resourceSetDescriptions
			.Select( rsd => device.ResourceFactory.CreateResourceSet( rsd ) )
			.ToArray();
	}

	private static bool IsParameterBuiltin( RenderBackend.Assets.MaterialParameter parameter, in int[] layouts )
	{
		return layouts.Contains( parameter.ResourceSetId );
	}

	private static DeviceBuffer CreateBufferForMaterialParameter( GraphicsDevice device, RenderBackend.Assets.MaterialParameter parameter, string? value )
	{
		if ( value is null )
		{
			return GetDefaultMaterialParameterBuffer( device, parameter.Type );
		}

		return null;
	}

	private static DeviceBuffer GetDefaultMaterialParameterBuffer( GraphicsDevice device, ShaderDataType type )
		=> type switch
		{
			ShaderDataType.Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, (byte)0 ),
			ShaderDataType.Short => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, (short)0 ),
			ShaderDataType.Int => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, 0 ),
			ShaderDataType.Float => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, 0.0f ),
			ShaderDataType.Vec2 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Vector2.Zero ),
			ShaderDataType.Vec3 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Vector3.Zero ),
			ShaderDataType.Vec4 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Vector4.Zero ),
			ShaderDataType.Vec2Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, new Vector2Byte( 0, 0 ) ),
			ShaderDataType.Vec3Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, new Vector3Byte( 0, 0, 0 ) ),
			ShaderDataType.Vec4Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Vector4B.Zero ),
			ShaderDataType.Mat44 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Matrix4x4.Identity ),
			_ => throw new NotSupportedException()
		};

	private static Texture CreateTextureForMaterialParameter( GraphicsDevice device, RenderBackend.Assets.MaterialParameter parameter, string? value )
	{
		if ( value is null )
		{
			return GetMissingTexture();
		}

		// TODO: SRGB hinting?
		var texture = Assets.LoadTexture( value, false );
		if ( texture is null )
		{
			return GetMissingTexture();
		}

		Debug.Assert( texture.RenderTexture is not null );
		return ((RenderTexture)texture.RenderTexture).DeviceTexture;
	}

	private static Texture GetMissingTexture()
	{
		return ((RenderTexture)Assets.MissingTexture.RenderTexture).DeviceTexture;
	}

	private GraphicsDevice mDevice;
	public Dictionary<string, ResourceSet[]> ResourceSets { get; private set; } = new();
	public List<RenderMaterialParameter> Parameters { get; private set; } = new();
	public MaterialDefinition Definition { get; init; }
	public MaterialTemplate Template { get; init; }

	public string[] GetParameterNames()
	{
		return Parameters.Select( p => p.Name ).ToArray();
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
		if ( TextureTypeCompatible( id, value.Width, value.Height, value.Depth ) )
		{
			Parameters[id].Texture = ((RenderTexture)value).DeviceTexture;
		}
	}

	private bool TextureTypeCompatible( int id, int width, int height, int depth )
	{
		if ( !ValidateIntention( id, ShaderDataType.Texture1D, ShaderDataType.Texture2D, ShaderDataType.Texture3D ) )
		{
			return false;
		}

		if ( Parameters[id].Texture is null )
		{
			return false;
		}

		var texture = Parameters[id].Texture;
		return Parameters[id].Texture.Type switch
		{
			TextureType.Texture1D => height == 0 && depth == 0,
			TextureType.Texture2D => height > 0 && depth == 0,
			TextureType.Texture3D => height > 0 && depth > 0,
			_ => false
		};
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
		=> Render.HasMaterialTemplate( materialDefinition.TemplateName ) switch
		{
			true => mMaterialSet.AddAndGet(
				new( mDevice, materialDefinition, Render.GetMaterialTemplate( materialDefinition.TemplateName ), [0, 1] ) ),
			false => null
		};

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
