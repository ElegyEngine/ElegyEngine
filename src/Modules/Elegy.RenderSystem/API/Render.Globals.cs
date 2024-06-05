// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.Resources;
using System.Numerics;
using Veldrid;

using MaterialParameterLevel = Elegy.RenderBackend.Assets.MaterialParameterLevel;
using ShaderDataType = Elegy.RenderBackend.Assets.ShaderDataType;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		static List<MaterialParameterSet> mGlobalParameters = new();

		private static bool ValidateIntention( GlobalParameterIndex id, params ShaderDataType[] types )
		{
			if ( id.GlobalParameterId >= mGlobalParameters.Count || id.GlobalParameterId < 0 )
			{
				mLogger.Error( $"Index is out of range (global param ID {id.GlobalParameterId})" );
				return false;
			}

			var globalParam = mGlobalParameters[id.GlobalParameterId];

			if ( id.ParameterId >= globalParam.Parameters.Count )
			{
				mLogger.Error( $"Index is out of range (global param {id.ParameterId} ID {id.ParameterId})" );
				return false;
			}

			var parameter = globalParam.Parameters[id.ParameterId];

			if ( !types.Contains( parameter.Type ) )
			{
				mLogger.Error( $"Type mismatch, parameter is {parameter.Type} but value is {types[0]} ({parameter.Name})" );
				return false;
			}

			return true;
		}

		private static bool TextureTypeCompatible( GlobalParameterIndex id, int width, int height, int depth )
		{
			if ( !ValidateIntention( id, ShaderDataType.Texture1D, ShaderDataType.Texture2D, ShaderDataType.Texture3D ) )
			{
				return false;
			}

			if ( GetParameter( id ).Texture is null )
			{
				return false;
			}

			var texture = GetParameter( id ).Texture;
			return GetParameter( id ).Texture.Type switch
			{
				TextureType.Texture1D => height == 0 && depth == 0,
				TextureType.Texture2D => height > 0 && depth == 0,
				TextureType.Texture3D => height > 0 && depth > 0,
				_ => false
			};
		}

		private static void RegenerateResourceSet( int globalParamId )
		{
			mGlobalParameters[globalParamId].RegenerateSet();
		}

		private static MaterialParameter GetParameter( GlobalParameterIndex id )
			=> mGlobalParameters[id.GlobalParameterId].Parameters[id.ParameterId];

		public static GlobalParameterIndex GetGlobalParameterIndex( string name )
		{
			for ( int globalParamId = 0; globalParamId < mGlobalParameters.Count; globalParamId++ )
			{
				var globalParam = mGlobalParameters[globalParamId];
				for ( int paramId = 0; paramId < globalParam.Parameters.Count; paramId++ )
				{
					var param = globalParam.Parameters[paramId];
					if ( param.Name == name )
					{
						return new()
						{
							GlobalParameterId = globalParamId,
							ParameterId = paramId
						};
					}
				}
			}

			return new()
			{
				GlobalParameterId = -1,
				ParameterId = -1
			};
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, int value )
		{
			if ( ValidateIntention( id, ShaderDataType.Int, ShaderDataType.Short, ShaderDataType.Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, float value )
		{
			if ( ValidateIntention( id, ShaderDataType.Float ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, bool value )
		{
			if ( ValidateIntention( id, ShaderDataType.Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, Vector2 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Vec2, ShaderDataType.Vec2Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, Vector3 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Vec3, ShaderDataType.Vec3Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, Vector4 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Vec4, ShaderDataType.Vec4Byte ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, Matrix4x4 value )
		{
			if ( ValidateIntention( id, ShaderDataType.Mat44 ) )
			{
				GetParameter( id ).SetValue( mDevice, value );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, RenderTexture value )
		{
			if ( TextureTypeCompatible( id, value.Width, value.Height, value.Depth ) )
			{
				GetParameter( id ).Texture = value.DeviceTexture;
				RegenerateResourceSet( id.GlobalParameterId );
			}
		}

		public static void SetGlobalParameter( GlobalParameterIndex id, Sampler value )
		{
			if ( ValidateIntention( id, ShaderDataType.Sampler ) )
			{
				GetParameter( id ).Sampler = value;
				RegenerateResourceSet( id.GlobalParameterId );
			}
		}

		public static MaterialParameterSet? GetGlobalParameterSet( string parameterName )
		{
			foreach ( var globalSet in mGlobalParameters )
			{
				foreach ( var globalParam in globalSet.Parameters )
				{
					if ( globalParam.Name == parameterName )
					{
						return globalSet;
					}
				}
			}

			return null;
		}

		public static MaterialParameterSet? GetGlobalParameterSet( RenderBackend.Assets.MaterialParameterSet set )
		{
			if ( set.Level != MaterialParameterLevel.Global )
			{
				return null;
			}

			var parameters = set.Parameters;

			foreach ( var globalSet in mGlobalParameters )
			{
				var globalParams = globalSet.Parameters;

				if ( parameters.Count != globalParams.Count )
				{
					continue;
				}

				bool compatible = true;
				for ( int i = 0; i < parameters.Count; i++ )
				{
					if ( parameters[i].Type != globalParams[i].Type )
					{
						compatible = false;
						break;
					}

					if ( parameters[i].Name != globalParams[i].Name )
					{
						compatible = false;
						break;
					}
				}

				if ( !compatible )
				{
					continue;
				}

				return globalSet;
			}

			return null;
		}

		public static MaterialParameter? GetGlobalParameter( string name )
		{
			foreach ( var globalSet in mGlobalParameters )
			{
				foreach ( var parameter in globalSet.Parameters )
				{
					if ( parameter.Name == name )
					{
						return parameter;
					}
				}
			}

			return null;
		}
	}
}
