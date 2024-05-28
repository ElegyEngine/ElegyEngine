// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.ConsoleSystem;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderBackend.Templating;
using Elegy.RenderSystem.API;

using System.Numerics;
using Veldrid;

using ShaderDataType = Elegy.RenderBackend.Assets.ShaderDataType;
using MaterialParameterLevel = Elegy.RenderBackend.Assets.MaterialParameterLevel;
using Utils = Elegy.RenderSystem.Resources.MaterialParameterUtils;

// TODO: regenerate resource sets when changing textures and samplers
// That will allow for toggling between pixelated thingies, anisotropic settings,
// and reloading textures while developing. It's gonna be a potentially costly operation,
// so none of that should be used during actual gameplay

namespace Elegy.RenderSystem.Resources
{
	public struct ResourceSetVariant
	{
		public ShaderVariant ShaderVariant { get; init; }
		public ResourceSet[] ResourceSets { get; init; }
	}

	public class MaterialParameterPool
	{
		private static TaggedLogger mLogger = new( "MaterialParams" );

		/// <summary>
		/// Constructor for global material parameters.
		/// </summary>
		public MaterialParameterPool( GraphicsDevice device, MaterialTemplate template )
		{
			mDevice = device;
			Template = template;

			ParameterLevel = MaterialParameterLevel.Global;
			// I arbitrarily chose 4 here, it's not common that there'll be more than that per material template
			Parameters.EnsureCapacity( 4 );

			foreach ( var parametre in template.ShaderTemplate.Parameters.AsSpan() )
			{
				if ( parametre.Level != MaterialParameterLevel.Global )
				{
					continue;
				}

				if ( parametre.Type == ShaderDataType.Buffer || parametre.Type == ShaderDataType.BufferRW )
				{
					mLogger.Warning( $"Global parametre '{parametre.Name}' is an unknown buffer type (global mat. param.: {parametre.Name})" );
					continue;
				}

				MaterialParameter? globalParam = Render.GetGlobalMaterialParameter( parametre.Name );
				if ( globalParam is null )
				{
					mLogger.Error( $"Global parametre '{parametre.Name}' cannot be found" );
					continue;
				}

				Parameters.Add( globalParam );
			}

			RegenerateParameterResourceSets();
		}

		/// <summary>
		/// Constructor for per-instance and data-driven material parameters.
		/// </summary>
		public MaterialParameterPool( GraphicsDevice device, MaterialTemplate materialTemplate, MaterialDefinition definition, bool perInstance = false )
		{
			mDevice = device;
			Definition = definition;
			Template = materialTemplate;

			ParameterLevel = perInstance ? MaterialParameterLevel.Data : MaterialParameterLevel.Instance;
			Parameters.EnsureCapacity( materialTemplate.ShaderTemplate.Parameters.Count );

			// From the shader parametres, we will look up the given material def's parametres
			// And then generate buffers for em
			foreach ( var parametre in materialTemplate.ShaderTemplate.Parameters.AsSpan() )
			{
				// Builtin and global parameters are filled in externally
				if ( parametre.Level != ParameterLevel )
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
					true => new( parametre.Name, parametre.Type, Utils.CreateTextureForMaterialParameter( device, parametre, value ) ),
					false => (parametre.Type == ShaderDataType.Sampler) switch
					{
						true => new( parametre.Name, Utils.GetSamplerByName( value ) ),
						false => new( parametre.Name, parametre.Type, Utils.CreateBufferForMaterialParameter( device, parametre, value ) )
					}
				} );
			}

			RegenerateParameterResourceSets();
		}

		private GraphicsDevice mDevice;
		public MaterialParameterLevel ParameterLevel { get; private set; }
		public List<ResourceSetVariant> ResourceSetVariants { get; private set; } = new();
		public List<MaterialParameter> Parameters { get; private set; } = new();
		public MaterialDefinition? Definition { get; private set; } = null;
		public MaterialTemplate Template { get; private set; }

		public int GetVariantIndex( string name )
		{
			for ( int i = 0; i < ResourceSetVariants.Count; i++ )
			{
				if ( ResourceSetVariants[i].ShaderVariant.Data.ShaderDefine == name )
				{
					return i;
				}
			}

			return -1;
		}

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
				RegenerateParameterResourceSets();
			}
		}

		public void SetSampler( int id, Sampler sampler )
		{
			if ( Parameters[id].Type == ShaderDataType.Sampler )
			{
				Parameters[id].Sampler = sampler;
				RegenerateParameterResourceSets();
			}
		}

		public void RegenerateParameterResourceSets()
		{
			ResourceSetVariants.EnsureCapacity( Template.ShaderVariants.Count );

			// Generate resource sets from material-level
			// parametres and shader variants
			foreach ( var variantAsset in Template.ShaderTemplate.ShaderVariants )
			{
				var variant = Template.ShaderVariants[variantAsset.ShaderDefine];
				ResourceSetVariants.Add( new()
				{
					ShaderVariant = variant,
					ResourceSets = Utils.GenerateResourceSetsForVariant( mDevice, variant, variantAsset, ParameterLevel, Parameters )
				} );
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
				mLogger.Error( $"Index is out of range ({id})" );
				return false;
			}

			if ( !types.Contains( Parameters[id].Type ) )
			{
				mLogger.Error( $"Type mismatch, parameter is {Parameters[id].Type} but value is {types[0]} ({Parameters[id].Name})" );
				return false;
			}

			return true;
		}

		public void Dispose()
		{

		}
	}
}
