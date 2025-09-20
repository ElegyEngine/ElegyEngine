// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Elegy.AssetSystem.API;
using Elegy.CommandSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Utilities;
using Elegy.FileSystem.API;
using Elegy.LogSystem.API;
using Elegy.PluginSystem.API;

namespace Elegy.Core;

public enum CoreStages
{
	PreConfig,
	LogSystem,
	CommandSystem,
	PluginSystem,
	FileSystem,
	AssetSystem,
	CoreGlue,
	PluginScan,
	PluginActivation,
	MaterialScan,
	FinalStep
}

/// <summary>
/// Starts up a minimal engine core for CLI apps, with just the logging system,
/// command system, virtual filesystem and plugin system.
/// </summary>
public static partial class CoreTemplate
{
	private static TaggedLogger mLogger = new( "Core" );
	private static Stopwatch mMainStopwatch = Stopwatch.StartNew();

	private static LaunchConfig mLaunchConfig;
	private static EngineConfig EngineConfig => mLaunchConfig.Engine;

	/// <summary>
	/// Seconds since the program started.
	/// </summary>
	public static double GetSeconds() => (double)mMainStopwatch.ElapsedTicks / Stopwatch.Frequency;

	/// <summary>
	/// Boots up the engine systems and starts off.
	/// </summary>
	public static bool Run( LaunchConfig config, Action main )
	{
		mMainStopwatch.Restart();
		return CreateStartupOrchestrator().Run( config, main );
	}

	/// <summary>
	/// Creates a startup <see cref="Orchestrator{T}"/> with a minimal set of engine systems.
	/// </summary>
	public static Orchestrator<LaunchConfig> CreateStartupOrchestrator()
		=> new Orchestrator<LaunchConfig>()
			// Load engine config, fix the working dir etc.
			.Add( CoreStages.PreConfig, DoPreConfig )
			// The order here doesn't matter too much, though ideally you want
			// your first three to be logging, the filesystem and the plugin
			// system. That's the basis for *everything* else
			.Add( CoreStages.LogSystem, InitLogging, Log.Shutdown )
			.Add( CoreStages.CommandSystem, InitCommands, Commands.Shutdown )
			.Add( CoreStages.FileSystem, InitFileSystem, Files.Shutdown )
			.Add( CoreStages.PluginSystem, InitPlugins, Plugins.Shutdown )
			.Add( CoreStages.AssetSystem, InitAssets, Assets.Shutdown )
			// Register plugin observers here as well as plugin
			// dependencies; and mount any needed paths
			.Add( CoreStages.CoreGlue, CoreGlueSetup, CoreGlueCleanup )
			.Add( CoreStages.PluginScan, ScanPlugins )
			// Instantiates and starts previously scanned plugins.
			// If you're gonna register plugin observers, do it before this!
			.Add( CoreStages.PluginActivation, ActivatePlugins )
			.Add( CoreStages.MaterialScan, ScanMaterials )
			.Add( CoreStages.FinalStep, DoFinalStep );

	private static bool DoPreConfig( LaunchConfig config )
	{
		if ( !LoadOrCreateEngineConfig( config ) )
		{
			return false;
		}

		SetupWorkingDirectory( config.Engine.EngineFolder );

		mLogger.Verbose( $"Working directory: '{Directory.GetCurrentDirectory()}'" );
		if ( EngineConfig.ConfigName != null )
		{
			mLogger.Developer( $"Engine configuration: '{EngineConfig.ConfigName}'" );
		}

		return true;
	}

	private static void DoFinalStep()
	{
		mLogger.Success( $"Startup time: {GetSeconds():F}s elapsed" );

		mLogger.Success( "Initialised Elegy Engine (no-version)" );
		mLogger.Warning( "This is an early in-development build of the engine. DO NOT use in production!" );
	}

	private static bool InitLogging( LaunchConfig config )
		=> Log.Init( config.ConsoleFrontends );

	private static bool InitCommands( LaunchConfig config )
	{
		if ( !Commands.Init( config.Args ) )
		{
			return false;
		}

		// Now that the command system parsed the commandline arguments, we can set some logging variables
		// TODO: The logging system has access to config.Args. Move this there ASAP
		Log.Verbose = Commands.Arguments.GetBool( "-verbose" );
		Log.Developer = Log.Verbose || Commands.Arguments.GetBool( "-dev" );

		return true;
	}

	private static bool InitFileSystem( LaunchConfig config )
		=> Files.Init( config.Engine );

	private static bool InitPlugins( LaunchConfig config )
		=> Plugins.Init( config.Engine.EngineFolder, config.Engine.EnginePlugins, config.ToolMode );

	private static bool InitAssets( LaunchConfig config )
		=> Assets.Init();
}
