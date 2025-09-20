// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderBackend.Templating;
using Elegy.RenderSystem.Resources;
using System.Text.Json;
using GlobalParameterSet = Elegy.RenderBackend.Assets.GlobalParameterSet;
using MaterialParameterLevel = Elegy.RenderBackend.Assets.MaterialParameterLevel;
using ShaderTemplate = Elegy.RenderBackend.Assets.ShaderTemplate;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		const string MaterialTemplatesDirectory = "materials/templates";
		const string ShaderTemplatesDirectory = "shaders";

		private static JsonSerializerOptions JsonOptions => RenderBackend.Text.MaterialTemplateJsonOptions.Instance;

		private static Dictionary<string, ShaderTemplate> mShaderTemplates = new();
		private static Dictionary<string, MaterialTemplate> mMaterialTemplates = new();

		public static IReadOnlyCollection<MaterialTemplate> MaterialTemplates => mMaterialTemplates.Values;

		public static void AddGlobalParameter( GlobalParameterSet parameterSetData )
		{
			List<MaterialParameter> materialParameters = parameterSetData.Parameters
				.Select( p => MaterialParameterUtils.CreateMaterialParameter( mDevice, p.Parameter.Name,
					p.Parameter.Type, p.DefaultValue ) )
				.ToList();

			Veldrid.ResourceLayout layout
				= Factory.CreateLayout( parameterSetData.Parameters.Select( p => p.Parameter ).ToList() );

			MaterialParameterSet set = new( mDevice, MaterialParameterLevel.Global, layout, materialParameters );
			mGlobalParameters.Add( set );
		}

		/// <summary>
		/// Loads a <see cref="ShaderTemplate"/>. Note: <paramref name="path"/> doesn't go through the VFS!
		/// </summary>
		public static bool LoadShaderTemplate( string path )
		{
			ShaderTemplate? template = JsonHelpers.LoadFrom<ShaderTemplate>( path, JsonOptions );
			if ( template is null )
			{
				mLogger.Warning( $"Can't load shader template '{path}'" );
				return false;
			}

			mShaderTemplates[template.Name] = template;
			return true;
		}

		public static bool LoadMaterialTemplate( string path )
		{
			var template = JsonHelpers.LoadFrom<RenderBackend.Assets.MaterialTemplate>( path, JsonOptions );
			if ( template is null )
			{
				mLogger.Warning( $"Can't load material template '{path}'" );
				return false;
			}

			if ( !mShaderTemplates.ContainsKey( template.ShaderTemplate ) )
			{
				mLogger.Warning(
					$"Can't load material template '{path}', it's trying to use a non-existing shader template '{template.ShaderTemplate}'" );
				return false;
			}

			mMaterialTemplates[template.Name] = new( template, mShaderTemplates[template.ShaderTemplate] );
			return true;
		}

		public static bool HasMaterialTemplate( string name )
		{
			return mMaterialTemplates.ContainsKey( name );
		}

		public static MaterialTemplate GetMaterialTemplate( string name )
		{
			return mMaterialTemplates[name];
		}
	}
}
