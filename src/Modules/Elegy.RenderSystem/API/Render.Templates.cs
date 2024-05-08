﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Extensions;
using Elegy.Common.Text;
using Elegy.FileSystem.API;
using Elegy.RenderBackend.Templating;

using ShaderTemplate = Elegy.RenderBackend.Assets.ShaderTemplate;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		const string MaterialTemplatesDirectory = "materials/mtemplates";
		const string ShaderTemplatesDirectory = "shaders";

		static Dictionary<string, ShaderTemplate> mShaderTemplates = new();
		static Dictionary<string, MaterialTemplate> mMaterialTemplates = new();
		
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

			return LoadMaterialTemplatesInDirectory( Files.CurrentGamePath )
				&& !anyFailed;
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
				ShaderTemplate? template = JsonHelpers.LoadFrom<ShaderTemplate>( entry );
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
				var template = JsonHelpers.LoadFrom<RenderBackend.Assets.MaterialTemplate>( entry );
				if ( template is null )
				{
					mLogger.Warning( $"Can't load material template '{entry}'" );
					numFailedMaterialTemplates++;
					continue;
				}
			}

			mLogger.WarningIf( numFailedShaderTemplates == 0,
				$"Failed to load {numFailedShaderTemplates} shader templates" );

			mLogger.WarningIf( numFailedMaterialTemplates == 0,
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
	}
}