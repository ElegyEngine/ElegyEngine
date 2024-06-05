// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;
using Elegy.FileSystem.API;
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

		static Dictionary<string, ShaderTemplate> mShaderTemplates = new();
		static Dictionary<string, MaterialTemplate> mMaterialTemplates = new();
		static List<MaterialParameterSet> mGlobalParameters = new();

		internal static bool LoadMaterialTemplates()
		{
			bool anyFailed = false;
			if ( !LoadMaterialTemplatesInDirectory( Files.EnginePath ) )
			{
				mLogger.Error( "Failed to load engine's material templates" );
				anyFailed = true;
			}

			foreach ( var mount in Files.CurrentConfig.Mounts )
			{
				if ( !LoadMaterialTemplatesInDirectory( mount ) )
				{
					mLogger.Error( $"Failed to load material templates in '{mount}'" );
					anyFailed = true;
				}
			}

			if ( anyFailed )
			{
				return false;
			}

			if ( !LoadMaterialTemplatesInDirectory( Files.CurrentGamePath ) )
			{
				return false;
			}

			return LoadGlobalParameters();
		}

		internal static bool LoadGlobalParameters()
		{
			string path = $"{ShaderTemplatesDirectory}/globalMaterialParams.json";
			mLogger.Developer( $"Loading global material parameters: '{path}'" );

			// TODO: Check all shader templates to see if any of them are using global parametres. If yes,
			// make this file required, else don't worry about it.
			string? fullPath = Files.PathTo( path, PathFlags.File );
			if ( fullPath is null )
			{
				mLogger.Warning( $"Global material parameters file missing! ({path})'" );
				return true;
			}

			List<GlobalParameterSet>? globalParams = JsonHelpers.LoadFrom<List<GlobalParameterSet>>( fullPath );
			if ( globalParams is null )
			{
				mLogger.Error( $"Cannot load global material parameters! ({path})'" );
				return false;
			}

			mGlobalParameters = globalParams.Select( globalParam =>
			{
				List<MaterialParameter> materialParameters = globalParam.Parameters
					.Select( p => MaterialParameterUtils.CreateMaterialParameter( mDevice, p.Parameter.Name, p.Parameter.Type, p.DefaultValue ) )
					.ToList();

				Veldrid.ResourceLayout layout = Factory.CreateLayout( globalParam.Parameters.Select( p => p.Parameter ).ToList() );
				
				return new MaterialParameterSet( mDevice, MaterialParameterLevel.Global, layout, materialParameters );
			} ).ToList();

			mLogger.Verbose( "Loaded global material parameters!" );
			return true;
		}

		internal static bool LoadMaterialTemplatesInDirectory( string directory )
		{
			mLogger.Developer( $"Loading shader & material templates: '{directory}'" );

			// Load shader templates
			var entries = Files.GetFiles( $"{directory}/{ShaderTemplatesDirectory}", "*.stemplate", recursive: true );
			if ( entries is null || entries.Length == 0 )
			{
				mLogger.WarningIf( directory == Files.EnginePath,
					"There's no shader templates in the engine folder!" );
				return true;
			}

			int numFailedShaderTemplates = 0;
			foreach ( var entry in entries )
			{
				ShaderTemplate? template = JsonHelpers.LoadFrom<ShaderTemplate>( entry, JsonOptions );
				if ( template is null )
				{
					mLogger.Warning( $"Can't load shader template '{entry}'" );
					numFailedShaderTemplates++;
					continue;
				}

				mShaderTemplates[template.Name] = template;
			}

			// Load material templates
			entries = Files.GetFiles( $"{directory}/{MaterialTemplatesDirectory}", "*.mtemplate", recursive: true );
			if ( entries is null || entries.Length == 0 )
			{
				mLogger.WarningIf( directory == Files.EnginePath,
					"There's no material templates in the engine folder!" );
				return true;
			}

			int numFailedMaterialTemplates = 0;
			foreach ( var entry in entries )
			{
				var template = JsonHelpers.LoadFrom<RenderBackend.Assets.MaterialTemplate>( entry, JsonOptions );
				if ( template is null )
				{
					mLogger.Warning( $"Can't load material template '{entry}'" );
					numFailedMaterialTemplates++;
					continue;
				}

				if ( !mShaderTemplates.ContainsKey( template.ShaderTemplate ) )
				{
					mLogger.Warning( $"Can't load material template '{entry}', it's trying to use a non-existing shader template '{template.ShaderTemplate}'" );
					numFailedMaterialTemplates++;
					continue;
				}

				mMaterialTemplates[template.Name] = new( template, mShaderTemplates[template.ShaderTemplate] );
			}

			mLogger.WarningIf( numFailedShaderTemplates != 0,
				$"Failed to load {numFailedShaderTemplates} shader templates" );

			mLogger.WarningIf( numFailedMaterialTemplates != 0,
				$"Failed to load {numFailedMaterialTemplates} material templates" );

			return numFailedShaderTemplates == 0 && numFailedMaterialTemplates == 0;
		}

		public static IReadOnlyCollection<MaterialTemplate> MaterialTemplates => mMaterialTemplates.Values;

		public static bool HasMaterialTemplate( string name )
		{
			return mMaterialTemplates.ContainsKey( name );
		}

		public static MaterialTemplate GetMaterialTemplate( string name )
		{
			return mMaterialTemplates[name];
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
