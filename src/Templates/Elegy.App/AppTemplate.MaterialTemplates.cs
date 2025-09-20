using Elegy.Common.Assets;
using Elegy.Common.Text;
using Elegy.FileSystem.API;
using Elegy.RenderBackend.Assets;
using Elegy.RenderSystem.API;

namespace Elegy.App;

public static partial class AppTemplate
{
	const string MaterialTemplatesDirectory = "materials/templates";
	const string ShaderTemplatesDirectory = "shaders";

	private static bool DoLoadMaterialTemplates( LaunchConfig config )
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

	private static bool LoadGlobalParameters()
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

		foreach ( var global in globalParams )
		{
			Render.AddGlobalParameter( global );
		}

		mLogger.Verbose( "Loaded global material parameters!" );
		return true;
	}

	private static bool LoadMaterialTemplatesInDirectory( string directory )
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
			if ( !Render.LoadShaderTemplate( entry ) )
			{
				numFailedShaderTemplates++;
			}
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
			if ( !Render.LoadMaterialTemplate( entry ) )
			{
				numFailedMaterialTemplates++;
			}
		}

		mLogger.ErrorIf( numFailedShaderTemplates != 0,
			$"Failed to load {numFailedShaderTemplates} shader templates" );

		mLogger.ErrorIf( numFailedMaterialTemplates != 0,
			$"Failed to load {numFailedMaterialTemplates} material templates" );

		return numFailedShaderTemplates == 0 && numFailedMaterialTemplates == 0;
	}
}
