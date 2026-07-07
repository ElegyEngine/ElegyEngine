using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Build.Evaluation;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;
using Project = Nuke.Common.ProjectModel.Project;

enum OptimisationProfile
{
	/// <summary> Do not optimise anything. </summary>
	None,

	/// <summary> Optimise the engine only. </summary>
	Engine,

	/// <summary> Optimise the project's dependencies too (physics lib, ECS lib...). </summary>
	EngineAndDeps,

	/// <summary> Optimise everything. </summary>
	All
}

class Build : NukeBuild
{
	public static int Main() => Execute<Build>( x => x.Instructions );

	[Parameter( "Optimise profile - Default is 'EngineAndDeps' (local) or 'All' (CI)" )]
	readonly OptimisationProfile Optimise = IsLocalBuild ? OptimisationProfile.EngineAndDeps : OptimisationProfile.All;

	Configuration EngineConfiguration => Optimise < OptimisationProfile.Engine ? Configuration.Debug : Configuration.Release;
	Configuration DependencyConfiguration => Optimise < OptimisationProfile.EngineAndDeps ? Configuration.Debug : Configuration.Release;
	Configuration PluginConfiguration => Optimise < OptimisationProfile.All ? Configuration.Debug : Configuration.Release;

	AbsolutePath SourceDir => RootDirectory / "src";
	AbsolutePath PluginsDir => SourceDir / "Plugins";

	AbsolutePath TestEnvDir => RootDirectory / "testgame";
	AbsolutePath GameDir => TestEnvDir / "game";
	AbsolutePath PluginsOutDir => GameDir / "plugins";

	Project LoadProject( string projectName )
		=> Solution.GetProject2( projectName ) ?? throw new KeyNotFoundException( $"{projectName}.csproj is missing from the solution" );

	Project GameProject => LoadProject( "Elegy.Game" );
	Project MapCompilerProject => LoadProject( "Elegy.MapCompiler" );
	Project ShaderToolProject => LoadProject( "Elegy.ShaderTool" );
	Project LauncherProject => LoadProject( "Elegy.Launcher2" );

	[Solution] readonly Solution Solution;
	[GitRepository] readonly GitRepository Repository;

	Target Instructions => _ => _
		.Executes( () =>
		{
			Log.Information( "Hello, welcome to the build system for Elegy ^w^" );
			Log.Information( "There's a few options for you:" );
			//Log.Information( "" );
			//Log.Information( "dotnet nuke Everything" );
			//Log.Information( "  This one will build everything (engine, launcher, game, plugins, shaders...)." );
			//Log.Information( "  It's designed for first-time setups and the like." );
			Log.Information( "" );
			Log.Information( "dotnet nuke Game" );
			Log.Information( "  Only builds the game, in DEBUG mode. You may use this one the most." );
			Log.Information( "" );
			Log.Information( "dotnet nuke Engine" );
			Log.Information( "  Builds the engine libraries & the launcher in RELEASE mode." );
			Log.Information( "" );
			Log.Information( "dotnet nuke Tools" );
			Log.Information( "  Builds the CLI tools in RELEASE mode." );
			//Log.Information( "" );
			//Log.Information( "dotnet nuke Shaders" );
			//Log.Information( "  Builds shaders." );
			//Log.Information( "" );
			//Log.Information( "dotnet nuke Plugins" );
			//Log.Information( "  Builds plugins, like the external console bridge etc." );
			Log.Information( "" );
			Log.Information( "NOTE: You can override debug/release stuff using the --optimise switch:" );
			Log.Information( "" );
			Log.Information( "--optimise none" );
			Log.Information( "  Everything is in DEBUG mode. Use this if working on the engine." );
			Log.Information( "" );
			Log.Information( "--optimise engine" );
			Log.Information( "  Engine modules (and tools) are in RELEASE mode. Plugins are in DEBUG mode. Use this if working on plugins." );
			Log.Information( "" );
			Log.Information( "--optimise engine-and-deps (DEFAULT)" );
			Log.Information(
				"  Same as above, except the plugin's direct dependencies are also in RELEASE mode. Use this if working on plugins and you need extra speed." );
			Log.Information( "" );
			Log.Information( "--optimise all" );
			Log.Information( "  Everything is in RELEASE mode. Use this when profiling, shipping etc." );
		} );

	Target Print => _ => _
		.Executes( () =>
		{
			Log.Information( "Git repository information" );
			Log.Information( "Commit: {Value}", Repository.Commit );
			Log.Information( "Branch: {Value}", Repository.Branch );
			Log.Information( "Tags:   {Value}", Repository.Tags );

			Log.Information( "Miscellaneous information" );
			Log.Information( "Solution: {Value}", Repository );
		} );

	Target Clean => _ => _
		.Executes( () =>
		{
		} );

	Target Engine => _ => _
		.Executes( () => Utils.Build( LauncherProject, EngineConfiguration ) )
		.Executes( () => Utils.CopyBinaries( LauncherProject, EngineConfiguration, TestEnvDir / "bin" ) );

	#region Game

	Target GameBuild => _ => _
		.Before( GameCopy )
		.Executes( () => Utils.Build( GameProject, DependencyConfiguration ) )
		.Executes( () =>
		{
			// If we intend to compile the engine/dependencies in release and the game/plugin in debug, we'll have to do this.
			// It's a little wasteful as we're building the game project twice, but it's very simple to implement
			if ( DependencyConfiguration != PluginConfiguration )
			{
				Utils.Build( GameProject, PluginConfiguration );
			}
		} );

	Target GameCopy => _ => _
		.Executes( () => Utils.CopyPluginBinaries( GameProject, DependencyConfiguration, PluginsOutDir / "Game" ) )
		.Executes( () => Utils.CopyBinary( GameProject, "Game.dll", PluginConfiguration, PluginsOutDir / "Game" ) );

	Target Game => _ => _
		.DependsOn( GameBuild, GameCopy )
		.Executes();

	#endregion

	#region Tools

	Target MapCompiler => _ => _
		.Executes( () => Utils.Build( MapCompilerProject, EngineConfiguration ) )
		.Executes( () => Utils.CopyBinaries( MapCompilerProject, EngineConfiguration, TestEnvDir / "bin" ) );

	Target ShaderTool => _ => _
		.Executes( () => Utils.Build( ShaderToolProject, EngineConfiguration ) )
		.Executes( () => Utils.CopyBinaries( ShaderToolProject, EngineConfiguration, TestEnvDir / "bin" ) );

	Target Tools => _ => _
		.DependsOn( MapCompiler, ShaderTool )
		.Executes();

	#endregion
}

static class Extensions
{
	[CanBeNull]
	public static Project GetProject2( this Solution solution, string projectName )
	{
		// For some weird reason, I cannot get solution.GetProject to work, it always returns null...
		foreach ( var proj in solution.GetAllProjects( "*" ) )
		{
			if ( proj.Name == projectName )
			{
				return proj;
			}
		}

		return null;
	}

	public static void CopyBinaryToDirectory( this AbsolutePath file, AbsolutePath output )
	{
		Log.Information( "Copying {0} to {1}...", file.Name, output );
		file.CopyToDirectory( output, ExistsPolicy.FileOverwriteIfNewer );

		AbsolutePath pdbPath = file.WithExtension( ".pdb" );
		if ( pdbPath.FileExists() )
		{
			Log.Information( "Copying {0} to {1}...", pdbPath.Name, output );
			pdbPath.CopyToDirectory( output, ExistsPolicy.FileOverwriteIfNewer );
		}
	}

	public static AbsolutePath GetOutputDirectory( this Project project, Configuration configuration )
	{
		return project.Directory / project.GetProperty( "OutputPath" )!
			.Replace( '\\', '/' )
			.Replace( "Debug", configuration );
	}

	public static (JsonDocument Json, AbsolutePath File) GetPluginConfig( this Project project )
	{
		AbsolutePath pluginConfigPath = project.Directory / "pluginConfig.json";
		if ( !pluginConfigPath.FileExists() )
		{
			Log.Error( "Project '{Value}' is missing a pluginConfig.json", project.Name );
			return (null, pluginConfigPath);
		}

		return (JsonDocument.Parse( pluginConfigPath.ReadAllText(), new()
		{
			AllowTrailingCommas = true,
			CommentHandling = JsonCommentHandling.Skip
		} ), pluginConfigPath);
	}

	public static IEnumerable<AbsolutePath> FindReferences( this Project project )
	{
		ICollection<ProjectItem> references = project.GetMSBuildProject().GetItems( "ProjectReference" );
		return references.Select( r => project.Directory / r.EvaluatedInclude );
	}
}

static class Utils
{
	public static PlatformID Platform => Environment.OSVersion.Platform;

	public static void Build( Project project, Configuration configuration )
	{
		Log.Information( "Building project {Value}...", project );

		DotNetTasks.DotNetBuild( config => config
			.SetConfiguration( configuration )
			.SetWarningLevel( 0 ) // clean logs pretty please...
			.SetVerbosity( DotNetVerbosity.quiet )
			.SetProjectFile( project )
		);
	}

	public static void CopyBinary( Project project, string binaryName, Configuration configuration, AbsolutePath output )
	{
		AbsolutePath outputPath = project.GetOutputDirectory( configuration );
		AbsolutePath file = outputPath / binaryName;

		file.CopyBinaryToDirectory( output );
	}

	public static void CopyBinaries( Project project, Configuration configuration, AbsolutePath output, string[] toSkip = null )
	{
		AbsolutePath outputPath = project.GetOutputDirectory( configuration );

		// A depth of 4 makes sure it scans "runtimes" as well
		var files = outputPath.GetFiles( depth: 4, attributes: FileAttributes.Normal );
		string assemblyName = project.GetProperty( "AssemblyName" );
		string runtimeDirectory = Environment.OSVersion.Platform switch
		{
			PlatformID.MacOSX => "osx",
			PlatformID.Unix => "linux-x64",
			_ => "win-x64"
		};

		foreach ( var file in files )
		{
			// If we're copying a plugin, ignore engine dependencies that end up in bin/
			if ( toSkip is not null && file.HasExtension( ".dll" ) )
			{
				if ( toSkip.Contains( file.NameWithoutExtension ) )
				{
					continue;
				}
			}

			// Currently just Silk.NET. Must check how others do it
			bool nativeDependency = file.Parent?.Name == "native" && file.ToString().Contains( runtimeDirectory );
			bool csharpAssemblyOrPdb = file.Parent?.Name != "native" && file.HasExtension( ".dll", ".pdb", ".exe" );
			bool runtimeConfig = file.Name.Contains( assemblyName ) && file.HasExtension( ".json" );
			bool linuxExecutable = Platform is PlatformID.Unix && file.Name == assemblyName;

			if ( nativeDependency || csharpAssemblyOrPdb || runtimeConfig || linuxExecutable )
			{
				Log.Information( "Copying {0} to {1}...", file.Name, output );
				file.CopyToDirectory( output, ExistsPolicy.FileOverwriteIfNewer );
			}
		}
	}

	public static void CopyPluginBinaries( Project project, Configuration configuration, AbsolutePath output )
	{
		// Read pluginConfig.json and copy its dependencies
		AbsolutePath binaryPath = project.GetOutputDirectory( configuration );

		var pluginConfig = project.GetPluginConfig();
		if ( pluginConfig.Json is null )
		{
			return;
		}

		JsonElement dependencyJson = pluginConfig.Json.RootElement.GetProperty( "dependencies" );
		foreach ( var dependency in dependencyJson.EnumerateArray() )
		{
			string dependencyString = dependency.GetString()!;

			AbsolutePath dependencyPath = binaryPath / dependencyString;
			if ( !dependencyPath.FileExists() )
			{
				Log.Error( "Project '{0}' does not have dependency '{1}' in its bin directory." +
				           "\nMake sure it's properly built or update pluginConfig.json, it may be out of date.",
					project.Name, dependencyString );
				continue;
			}

			dependencyPath.CopyBinaryToDirectory( output );
		}

		Log.Information( "Copying {0} to {1}...", "pluginConfig.json", output );
		pluginConfig.File.CopyToDirectory( output, ExistsPolicy.FileOverwriteIfNewer );
	}
}
