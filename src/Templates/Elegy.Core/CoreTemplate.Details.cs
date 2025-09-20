// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.CommandSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.FileSystem.API;
using Elegy.LogSystem.API;
using Elegy.LogSystem.Frontends;
using Elegy.PluginSystem.API;

namespace Elegy.Core;

public static partial class CoreTemplate
{
	private static bool CoreGlueSetup( LaunchConfig config )
	{
		Plugins.RegisterDependency( "Elegy.AssetSystem", typeof( Assets ).Assembly );
		Plugins.RegisterDependency( "Elegy.CommandSystem", typeof( Commands ).Assembly );
		Plugins.RegisterDependency( "Elegy.Core", typeof( CoreTemplate ).Assembly );
		Plugins.RegisterDependency( "Elegy.FileSystem", typeof( Files ).Assembly );
		Plugins.RegisterDependency( "Elegy.LogSystem", typeof( Log ).Assembly );

		foreach ( var loader in Assets.ModelLoaders )
		{
			Plugins.RegisterPlugin( loader );
		}

		foreach ( var loader in Assets.TextureLoaders )
		{
			Plugins.RegisterPlugin( loader );
		}

		foreach ( var loader in Assets.LevelLoaders )
		{
			Plugins.RegisterPlugin( loader );
		}

		foreach ( var loader in Assets.LevelWriters )
		{
			Plugins.RegisterPlugin( loader );
		}

		// Register the plugin collector *after* registering the plugins,
		// so that builtin loaders aren't loaded twice
		Plugins.RegisterPluginCollector( new AssetPluginCollector() );

		return true;
	}

	private static void CoreGlueCleanup()
	{
		Plugins.UnregisterPluginCollector<AssetPluginCollector>();
		Plugins.UnregisterDependency( "Elegy.AssetSystem" );
		Plugins.UnregisterDependency( "Elegy.CommandSystem" );
		Plugins.UnregisterDependency( "Elegy.Core" );
		Plugins.UnregisterDependency( "Elegy.FileSystem" );
		Plugins.UnregisterDependency( "Elegy.LogSystem" );
	}

	private static bool ScanPlugins( LaunchConfig config )
	{
		string engineFolder = config.Engine.EngineFolder;
		string[] enginePlugins = config.Engine.EnginePlugins;
		bool toolMode = config.ToolMode;

		// The game etc. will be loaded additionally, when
		// mounted by the file system. OR it is injected
		bool someEnginePluginsFailed = false;
		foreach ( string enginePluginPath in enginePlugins )
		{
			if ( !Plugins.ScanLibrary( $"{engineFolder}/{enginePluginPath}/pluginConfig.json" ) )
			{
				someEnginePluginsFailed = true;
			}
		}

		if ( someEnginePluginsFailed )
		{
			mLogger.Error( "One or more engine plugins couldn't load!" );
		}

		if ( !toolMode )
		{
			bool someGamePluginsFailed = false;
			foreach ( string gamePlugin in Files.CurrentConfig.Plugins )
			{
				if ( !Plugins.ScanLibrary( $"{Files.CurrentGamePath}/{gamePlugin}/pluginConfig.json" ) )
				{
					someGamePluginsFailed = true;
				}
			}

			if ( someGamePluginsFailed )
			{
				mLogger.Warning( "One or more base game plugins couldn't load, some things may not work!" );
			}
		}

		return !someEnginePluginsFailed;
	}

	private static bool ActivatePlugins( LaunchConfig config )
	{
		if ( !Plugins.ActivatePlugins() )
		{
			return false;
		}

		foreach ( IPlugin plugin in Plugins.GenericPlugins )
		{
			if ( plugin is IConsoleFrontend frontend )
			{
				Log.AddFrontend( frontend );
			}
		}

		return true;
	}

	private static bool ScanMaterials( LaunchConfig config )
	{
		if ( !Assets.PostInit() )
		{
			return false;
		}

		int totalMaterials = 0;

		// TODO: This particular search for materials should be
		// part of a plugin, read note in AssetSystem
		bool LoadMaterialsForDirectory( string name, string directory )
		{
			string? path = Files.PathTo( $"{directory}/materials", PathFlags.Directory );
			if ( path is null )
			{
				mLogger.Error( $"{name} directory doesn't exist or doesn't have any materials!" );
				return false;
			}

			var materialDocumentPaths = Files.GetEntries( path, "*.shader", PathFlags.File, recursive: true );
			if ( materialDocumentPaths is null || materialDocumentPaths.Length == 0 )
			{
				mLogger.Error( $"{name}'s materials directory is empty!" );
				return false;
			}

			foreach ( var materialDocumentPath in materialDocumentPaths )
			{
				MaterialDocument document = new( File.ReadAllText( materialDocumentPath ) );
				if ( document.Materials.Count == 0 )
				{
					mLogger.Warning( $"Parsed 0 materials in '{materialDocumentPath}'" );
					continue;
				}

				Assets.AddMaterialDocument( document );
				mLogger.Developer( $"Parsed {document.Materials.Count} materials in '{materialDocumentPath}'" );
				totalMaterials += document.Materials.Count;
			}

			return true;
		}

		mLogger.Log( "Loading engine materials..." );
		if ( !LoadMaterialsForDirectory( "Engine", Files.EnginePath ) )
		{
			return false;
		}

		foreach ( var mount in Files.CurrentConfig.Mounts )
		{
			LoadMaterialsForDirectory( $"Mounted game {mount}", mount );
		}

		LoadMaterialsForDirectory( "This game", Files.CurrentGamePath );

		mLogger.Success( $"Parsed {totalMaterials} materials!" );
		return true;
	}
}
