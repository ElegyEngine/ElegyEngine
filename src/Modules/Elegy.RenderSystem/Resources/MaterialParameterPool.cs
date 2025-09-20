// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Interfaces.Rendering;
using Elegy.RenderBackend.Templating;
using Elegy.RenderSystem.API;

using System.Numerics;
using Elegy.Common.Utilities;
using Veldrid;

using ShaderDataType = Elegy.RenderBackend.Assets.ShaderDataType;
using MaterialParameterLevel = Elegy.RenderBackend.Assets.MaterialParameterLevel;
using Utils = Elegy.RenderSystem.Resources.MaterialParameterUtils;

namespace Elegy.RenderSystem.Resources
{
	// TODO: Get rid of this, it's basically not used, but the shader variant
	// lookup code relies on this for shader variant indices. It's not right
	// and can be done much faster
	public struct ResourceSetVariant
	{
		public ShaderVariant ShaderVariant { get; init; }
		public int[] ResourceSetIds { get; init; }
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
			ParameterSets.EnsureCapacity( 4 );

			foreach ( var set in template.ShaderTemplate.ParameterSets.AsSpan() )
			{
				if ( set.Level != MaterialParameterLevel.Global )
				{
					continue;
				}

				MaterialParameterSet? globalSet = Render.GetGlobalParameterSet( set );
				if ( globalSet is null )
				{
					mLogger.Error( $"Cannot find global parametre '{set.Parameters[0].Name}'" );
					continue;
				}

				ParameterSets.Add( globalSet );
			}
		}

		/// <summary>
		/// Constructor for per-instance and data-driven material parameters.
		/// </summary>
		public MaterialParameterPool( GraphicsDevice device, MaterialTemplate materialTemplate, MaterialDefinition definition, bool perInstance = false )
		{
			mDevice = device;
			Definition = definition;
			Template = materialTemplate;

			ParameterLevel = perInstance ? MaterialParameterLevel.Instance : MaterialParameterLevel.Data;
			ParameterSets.EnsureCapacity( materialTemplate.ShaderTemplate.ParameterSets.Count );

			// From the shader parametres, we will look up the given material def's parametres
			// And then generate buffers for em
			int parameterSetId = -1;
			foreach ( var set in materialTemplate.ShaderTemplate.ParameterSets.AsSpan() )
			{
				parameterSetId++;

				// Builtin and global parameters are filled in externally
				if ( set.Level != ParameterLevel )
				{
					continue;
				}

				List<MaterialParameter> parameters = new( set.Parameters.Count );
				foreach ( var parametre in set.Parameters.AsSpan() )
				{
					if ( parametre.Type == ShaderDataType.Buffer || parametre.Type == ShaderDataType.BufferRW )
					{
						mLogger.Error( $"Parametre '{parametre.Name}' is an unknown buffer type (mat: {definition.Name})" );
						continue;
					}

					string? value = definition.Parameters.GetValueOrDefault( parametre.Name );
					parameters.Add( Utils.CreateMaterialParameter( device, parametre.Name, parametre.Type, value ) );
				}

				MaterialParameterSet parameterSet = new( mDevice, ParameterLevel, materialTemplate.ResourceLayouts[parameterSetId], parameters );
				ParameterSets.Add( parameterSet );
			}

			RegenerateParameterResourceSets();
		}

		private GraphicsDevice mDevice;
		public MaterialParameterLevel ParameterLevel { get; private set; }
		public List<ResourceSetVariant> ResourceSetVariants { get; private set; } = new();
		public List<MaterialParameterSet> ParameterSets { get; private set; } = new();
		public MaterialDefinition? Definition { get; private set; } = null;
		public MaterialTemplate Template { get; private set; }

		public int GetVariantIndex( string name )
		{
			int i = 0;
			foreach ( var variant in Template.ShaderVariants.Values )
			{
				if ( variant.Data.ShaderDefine == name )
				{
					return i;
				}

				i++;
			}

			return -1;
		}

		public string[] GetParameterNames()
		{
			return ParameterSets.SelectMany( set => set.Parameters.Select( param => param.Name ) ).ToArray();
		}

		public MaterialParameter GetParameter( int id )
		{
			return ParameterSets[id % 100].Parameters[id / 100];
		}

		public int GetParameterIndex( string name )
		{
			for ( int setId = 0; setId < ParameterSets.Count; setId++ )
			{
				var set = ParameterSets[setId];

				for ( int paramId = 0; paramId < set.Parameters.Count; paramId++ )
				{
					if ( set.Parameters[paramId].Name == name )
					{
						return setId + paramId * 100;
					}
				}
			}

			return -1;
		}

		public void SetParameter( int id, int value )
		{
			if ( ValidateIntention( id, ShaderDataType.Int, ShaderDataType.Short, ShaderDataType.Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetParameter( int id, float value )
		{
			if ( ValidateIntention( id, ShaderDataType.Float ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetParameter( int id, bool value )
		{
			if ( ValidateIntention( id, ShaderDataType.Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetParameter( int id, Vector2 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Vec2, ShaderDataType.Vec2Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetParameter( int id, Vector3 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Vec3, ShaderDataType.Vec3Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetParameter( int id, Vector4 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Vec4, ShaderDataType.Vec4Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetParameter( int id, Matrix4x4 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Mat44 ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public void SetBufferParameter<T>( int id, T bufferValue ) where T : unmanaged
		{
			if ( ValidateIntention( id, ShaderDataType.Buffer, ShaderDataType.BufferRW ) )
			{
				GetParameter( id ).SetBufferValue( mDevice, bufferValue );
			}
		}

		public void SetTexture( int id, ITexture value )
		{
			if ( TextureTypeCompatible( id, value.Width, value.Height, value.Depth ) )
			{
				GetParameter( id ).Texture = ((RenderTexture)value).DeviceTexture;
				RegenerateResourceSet( id );
			}
		}

		public void SetSampler( int id, Sampler sampler )
		{
			if ( GetParameter( id ).Type == ShaderDataType.Sampler )
			{
				GetParameter( id ).Sampler = sampler;
				RegenerateResourceSet( id );
			}
		}

		public void RegenerateResourceSet( int id )
		{
			ParameterSets[id].RegenerateSet();
		}

		public void RegenerateParameterResourceSets()
		{
			ResourceSetVariants.EnsureCapacity( Template.ShaderVariants.Count );

			for ( int i = 0; i < ParameterSets.Count; i++ )
			{
				RegenerateResourceSet( i );
			}

			// Generate resource sets from material-level
			// parametres and shader variants
			foreach ( var variantAsset in Template.ShaderTemplate.ShaderVariants )
			{
				var variant = Template.ShaderVariants[variantAsset.ShaderDefine];
				ResourceSetVariants.Add( new()
				{
					ShaderVariant = variant,
					ResourceSetIds = variantAsset.ParameterSetIds.ToArray()
				} );
			}
		}

		private bool TextureTypeCompatible( int id, int width, int height, int depth )
		{
			if ( !ValidateIntention( id, ShaderDataType.Texture1D, ShaderDataType.Texture2D, ShaderDataType.Texture3D ) )
			{
				return false;
			}

			if ( GetParameter( id ).Texture is null )
			{
				return false;
			}

			return GetParameter( id ).Texture?.Type switch
			{
				TextureType.Texture1D => height == 0 && depth == 1,
				TextureType.Texture2D => height > 0 && depth == 1,
				TextureType.Texture3D => height > 0 && depth > 1,
				_ => false
			};
		}

		private bool ValidateIntention( int id, params ShaderDataType[] types )
		{
			int setId = id % 100;
			int paramId = id / 100;

			if ( setId >= ParameterSets.Count || id < 0 )
			{
				mLogger.Error( $"Index is out of range (set {setId} param {paramId})" );
				return false;
			}

			if ( paramId >= ParameterSets[setId].Parameters.Count )
			{
				mLogger.Error( $"Index is out of range ({id})" );
				return false;
			}

			var parameter = ParameterSets[setId].Parameters[paramId];

			if ( !types.Contains( parameter.Type ) )
			{
				mLogger.Error( $"Type mismatch, parameter is {parameter.Type} but value is {types[0]} ({parameter.Name})" );
				return false;
			}

			return true;
		}

		public void Dispose()
		{
			foreach ( var set in ParameterSets )
			{
				set.Dispose();
			}
		}
	}
}
