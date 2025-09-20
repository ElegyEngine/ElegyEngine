using System.Reflection;
using Elegy.Common.Assets;

namespace Elegy.Core;

public static partial class CoreTemplate
{
	private static bool LoadOrCreateEngineConfig( in LaunchConfig config )
	{
		mLaunchConfig = config;
		if ( config.EngineConfigName is null )
		{
			return true;
		}

		if ( !File.Exists( config.EngineConfigName ) )
		{
			mLogger.Log( $"'{config.EngineConfigName}' does not exist, creating a default one..." );

			mLaunchConfig.Engine = new();
			Common.Text.JsonHelpers.Write( EngineConfig, config.EngineConfigName );
			return true;
		}

		EngineConfig engineConfig = new();
		if ( !Common.Text.JsonHelpers.LoadFrom( ref engineConfig, config.EngineConfigName ) )
		{
			mLogger.Error( $"'{config.EngineConfigName}' somehow failed to load" );
			return false;
		}

		mLaunchConfig.Engine = engineConfig;

		return true;
	}

	private static void SetupWorkingDirectory( string engineFolder )
	{
		bool VerifyDirectory( string path )
		{
			foreach ( var fullDirectory in Directory.GetDirectories( path ) )
			{
				string directory = Path.GetRelativePath( path, fullDirectory );

				// If there's an 'engine' folder in there, then it's a safe guarantee
				if ( directory == engineFolder )
				{
					return true;
				}

				// TODO: recognise applicationConfig.json and others
			}

			return false;
		}

		bool ScanUpward( string path )
		{
			// Situation when this works:
			// testgame/Elegy.Launcher2.exe
			// testgame/engine/
			if ( VerifyDirectory( path ) )
			{
				Directory.SetCurrentDirectory( path );
				return true;
			}

			// In case this didn't work out, we try moving up to 2 levels up
			// testgame/bin/Elegy.Launcher2.exe
			// testgame/engine/
			// But also:
			// testgame/game/bin/Elegy.Launcher2.exe
			// testgame/engine/
			for ( int i = 0; i < 2; i++ )
			{
				path = Path.Combine( path, ".." );

				if ( VerifyDirectory( path ) )
				{
					Directory.SetCurrentDirectory( path );
					return true;
				}
			}

			return false;
		}

		if ( ScanUpward( Directory.GetCurrentDirectory() ) )
		{
			return;
		}

		// If moving up the working dir fails, then we
		// can only really look from the DLL's location
		Assembly currentAssembly = typeof( CoreTemplate ).GetTypeInfo().Assembly;
		string currentAssemblyDirectory = Directory.GetParent( currentAssembly.Location )!.FullName;
		ScanUpward( currentAssemblyDirectory );
	}
}
