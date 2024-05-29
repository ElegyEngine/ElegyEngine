// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Extensions;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.RenderBackend.Assets;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.API;
using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace Elegy.RenderSystem.Resources
{
	public static class MaterialParameterUtils
	{
		public static ResourceSet GenerateResourceSet( GraphicsDevice device, ResourceLayout layout, List<MaterialParameter> parameters )
		{
			// This loop generates the elements for a ResourceSet
			BindableResource[] bindableResources = new BindableResource[parameters.Count];
			for ( int i = 0; i < parameters.Count; i++ )
			{
				MaterialParameter parameter = parameters[i];

				BindableResource bindableResource = parameter.Type switch
				{
					ShaderDataType.Texture1D or
					ShaderDataType.Texture2D or
					ShaderDataType.Texture3D => parameter.Texture,

					ShaderDataType.Sampler => parameter.Sampler,

					_ => parameter.Buffer
				};

				bindableResources[i] = bindableResource;
			}

			return device.ResourceFactory.CreateResourceSet( new()
			{
				Layout = layout,
				BoundResources = bindableResources
			} );
		}

		public static DeviceBuffer CreateBufferForMaterialParameter( GraphicsDevice device, ShaderDataType type, string? value )
		{
			if ( value is null )
			{
				return GetDefaultMaterialParameterBuffer( device, type );
			}

			return type switch
			{
				ShaderDataType.Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, (byte)Parse.Int( value ) ),
				ShaderDataType.Short => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, (short)Parse.Int( value ) ),
				ShaderDataType.Int => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Parse.Int( value ) ),
				ShaderDataType.Float => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, Parse.Float( value ) ),
				ShaderDataType.Vec2 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, value.ToVector2() ),
				ShaderDataType.Vec3 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, value.ToVector3() ),
				ShaderDataType.Vec4 => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, value.ToVector4() ),
				// Byte variants are written as 0-255 in the file, so when we parse them, convert to 0-1 so they can be converted to 0-255 again internally.
				// Wew. Could've just written some parsing code for these fellas, but oh well!
				ShaderDataType.Vec2Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, (Vector2Byte)(value.ToVector2() / 255.0f) ),
				ShaderDataType.Vec3Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, (Vector3Byte)(value.ToVector3() / 255.0f) ),
				ShaderDataType.Vec4Byte => device.CreateBufferFromStruct( BufferUsage.UniformBuffer, new Vector4B( value.ToVector4() / 255.0f ) ),
				_ => throw new NotSupportedException()
			};
		}

		public static DeviceBuffer GetDefaultMaterialParameterBuffer( GraphicsDevice device, ShaderDataType type )
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

		public static Sampler GetSamplerByName( string? value )
		{
			if ( value is null )
			{
				return Render.Samplers.Linear;
			}

			return value.ToLower() switch
			{
				"nearest" => Render.Samplers.Nearest,
				"linear" => Render.Samplers.Linear,
				"aniso2" => Render.Samplers.Aniso2x,
				"aniso4" => Render.Samplers.Aniso4x,
				"aniso8" => Render.Samplers.Aniso8x,
				"aniso16" => Render.Samplers.Aniso16x,

				"nearestclamp" => Render.Samplers.NearestClamp,
				"linearclamp" => Render.Samplers.LinearClamp,
				"aniso2clamp" => Render.Samplers.Aniso2xClamp,
				"aniso4clamp" => Render.Samplers.Aniso4xClamp,
				"aniso8clamp" => Render.Samplers.Aniso8xClamp,
				"aniso16clamp" => Render.Samplers.Aniso16xClamp,

				"nearestborder" => Render.Samplers.NearestBorder,
				"linearborder" => Render.Samplers.LinearBorder,
				"aniso2border" => Render.Samplers.Aniso2xBorder,
				"aniso4border" => Render.Samplers.Aniso4xBorder,
				"aniso8border" => Render.Samplers.Aniso8xBorder,
				"aniso16border" => Render.Samplers.Aniso16xBorder,

				_ => Render.Samplers.Linear
			};
		}

		public static Texture CreateTextureForMaterialParameter( GraphicsDevice device, string? value )
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

		public static MaterialParameter CreateMaterialParameter( GraphicsDevice device, string name, ShaderDataType type, string? value )
		{
			return type.IsTexture() switch
			{
				true => new( name, type, CreateTextureForMaterialParameter( device, value ) ),
				false => (type == ShaderDataType.Sampler) switch
				{
					true => new( name, GetSamplerByName( value ) ),
					false => new( name, type, CreateBufferForMaterialParameter( device, type, value ) )
				}
			};
		}

		public static Texture GetMissingTexture()
		{
			return ((RenderTexture)Assets.MissingTexture.RenderTexture).DeviceTexture;
		}
	}
}
