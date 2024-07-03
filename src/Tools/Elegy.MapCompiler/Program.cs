// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// Overview of Elegy.MapCompiler's responsibilities & function
//
// ===  Geo  === (WiP)
// 1. Load the .map file and intersect planes to get the base mesh (done)
// 2. Post-process (removing bad triangles, edge smoothing etc.)
// 3. Subdivide the map into a double grid
// 4. Generate the output meshes (visual, collision, occluders)
//
// ===  Vis  === (not done)
// 1. Spawn tons of cameras in rooms, remove ones that are considered to be in the void
// 2. Perform visibility computation via rasterisation on the GPU
// 3. Write output visibility data
//
// === Light === (not done)
// 1. Generate UVs for lightmap
// 2. Fill out one or more WorldPosition and WorldNormal maps
// 3. Perform lighting calculations via
//		a) ray-tracing on the GPU
//		b) ray-tracing on the CPU
// 4. Update visual mesh with lightmap texture names

using Elegy.Common.Assets;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;
using Elegy.Framework;
using Elegy.Framework.Bootstrap;
using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.ConsoleArguments;
using Elegy.MapCompiler.Data.Processing;
using Elegy.MapCompiler.Processors;

namespace Elegy.MapCompiler
{
	[ElegyBootstrap]
	[WithAssetSystem]
	[WithConsoleSystem]
	[WithFileSystem]
	public static partial class Program
	{
		private static MapCompilerParameters mParameters = new();
		private static TaggedLogger mLogger = new( "MapCompiler" );

		public static void Main( string[] args )
		{
			if ( !ProcessArgs( args ) )
			{
				PrintUsage();
				return;
			}

			if ( mParameters.DebugFreeze )
			{
				DebugFreeze();
			}

			System.Console.WriteLine( "Init" );

			if ( mParameters.MapFile == string.Empty )
			{
				// The engine is still uninitialized, so we must use System.Console instead.
				System.Console.WriteLine( "No map file was provided. Specify one with '-map path/to/file.map'." );
				return;
			}

			if ( mParameters.RootPath == string.Empty )
			{
				// Guess the root directory from the map file path.
				string? mapDirectoryName = Path.GetDirectoryName( mParameters.MapFile );
				if ( mapDirectoryName != null && Path.IsPathRooted( mParameters.MapFile ) )
				{
					DirectoryInfo? dir = new( mapDirectoryName );
					// Get the 'maps' folder in the game folder.
					while ( dir != null && dir.Name != "maps" )
					{
						dir = dir.Parent;
					}
					// Now get the root folder. (Since the maps folder is at '/root/game/maps'.)
					dir = dir?.Parent?.Parent;
					// We have a candidate for the root directory. Let's try it!
					if ( dir != null )
					{
						Directory.SetCurrentDirectory( dir.FullName );
						System.Console.WriteLine( $"No root folder specified: Guessing from the map file path that it is '{dir.FullName}'." );
					}
				}
				else
				{
					System.Console.WriteLine( "No root folder specified. Using the current working directory as the root:" );
					System.Console.WriteLine( Directory.GetCurrentDirectory() );
				}
			}
			else
			{
				Directory.SetCurrentDirectory( mParameters.RootPath );
			}

			// The file system needs a relative path, so convert the map file path if it isn't one.
			if ( Path.IsPathRooted( mParameters.MapFile ) )
			{
				mParameters.MapFile = Path.GetRelativePath( Directory.GetCurrentDirectory(), mParameters.MapFile );
			}

			LaunchConfig config = new()
			{
				ToolMode = true
			};

			if ( !EngineSystem.Init( config,
					   systemInitFunc: Init_Generated,
					   systemPostInitFunc: PostInit_Generated,
					   systemShutdownFunc: Shutdown_Generated,
					   systemErrorFunc: ErrorMessage_Generated ) )
			{
				mLogger.Fatal(
					EngineSystem.ShutdownReason is null
					? "Engine failed to initialise: reason unknown"
					: $"Engine failed to initialise: '{EngineSystem.ShutdownReason}'" );

				mLogger.Error( "Failed to compile map." );
				return;
			}

			if ( mParameters.MapFile == string.Empty )
			{
				mLogger.Error( "No map file was provided. Specify one with '-map path/to/file.map'." );
				return;
			}

			string? mapPath = Files.PathTo( mParameters.MapFile );
			if ( mapPath == null )
			{
				mLogger.Error( $"""
							   The specified map file '{mapPath}' does not exist or cannot be accessed.
							   Please check that the path is correct and that you are in the correct root folder. (Currently: '{Directory.GetCurrentDirectory()}')
							   You can specify a custom root folder with '-root path/to/root'.
							   """ );
				return;
			}

			// If the output path wasn't provided, then we assume the user wants to output to the same location as the .map
			if ( mParameters.OutputPath == string.Empty )
			{
				mParameters.OutputPath = Path.ChangeExtension( mParameters.MapFile, ".elf" );
			}
			else
			{
				mParameters.OutputPath = Path.Join( Path.GetDirectoryName( mapPath ) ?? "", Path.ChangeExtension( mParameters.OutputPath, ".elf" ) );
			}

			BrushMapDocument? document = null;
			try
			{
				document = BrushMapDocument.FromValve220MapFile( mapPath );
			}
			catch ( Exception ex )
			{
				mLogger.Error( $"""
								An error occured while parsing the map file!
								Message: {ex.Message}.

								An error while parsing the map means something went fundementally wrong.
								You can try opening the map file in a text editor and checking that line & column for an anomaly.

								If this is a perfectly valid .map file (i.e. it is parsed fine by other compilers),
								then please open an issue on our GitHub repository:
								https://github.com/ElegyEngine/ElegyEngine/issues
								Thank you for your patience. :3
								""" );
				return;
			}

			if ( document is null )
			{
				mLogger.Fatal( "An unknown error occured." );
				return;
			}

			ProcessingData data = new();
			GeometryProcessor processor = new( data, mParameters );
			processor.GenerateGeometryFromMap( document );
			processor.FixBrushOrigins();
			processor.UpdateBoundaries();
			//processor.SmoothenNormals();
			//processor.GenerateDualGrid();

			//VisibilityProcessor vp = new( data, mParameters );
			//vp.ProcessVisibility();

			//LightProcessor lp = new( data, mParameters );
			//lp.GenerateLightmapUvs();
			//lp.GenerateLightmapImages();
			//lp.ProcessLighting();

			OutputProcessor op = new( data, mParameters );
			op.GenerateOutputData();
			op.WriteToFile( mParameters.OutputPath );

			mLogger.Success( $"Map compiled successfully. The result can be found at '{mParameters.OutputPath}'." );
		}

		private static void DebugFreeze()
		{
			System.Console.Write( """
			                      DEVELOPER: You have entered the 'debug freeze'. The map compiler will now freeze until a debugger is attached.
			                      You can quit the map compiler by pressing Ctrl+C or closing the console window.
			                      """ );

			while ( true )
			{
				Thread.Sleep( 250 );

				if ( !System.Diagnostics.Debugger.IsAttached )
				{
					continue;
				}

				System.Console.WriteLine( "Debugger attached! Continuing..." );
				Thread.Sleep( 500 );
				return;
			}
		}

		private static bool ProcessArgs( string[] args )
		{
			if ( args.Length == 0 )
			{
				mLogger.Error( "No arguments were provided." );
				return false;
			}

			if ( args.Length == 1 && args[0] == "-help" )
			{
				mLogger.Log( "Ah, a lost soul like myself. Let me guide you..." );
				return false;
			}

			ParameterManager.ProcessArguments( args, out mParameters );
			return true;
		}

		private static void PrintUsage()
		{
			const string message = """
			                       In order to compile a map, you need to call me like so:
			                       > Elegy.MapCompiler -map "game/maps/mymap.map" -root "C:/MyGame/game"

			                       Now, here's a list of all parameters:
			                       | NAME          | DESCRIPTION
			                       
			                         -map:           Path to the map, preferably absolute, can be relative to the game directory.
			                         
			                         -out:           Path to the resulting compiled map file, relative to the map file's directory.
			                                         If left empty, I'll use the same path as the map file, but with a different extension.
			                         
			                         -root:          Path to the root directory, from which all assets are pulled.
			                                         If left empty, I'll try to guess the root directory from the provided path to the map file.
			                                         E.g. If you specify "-map 'C:/MyGame/game/maps/mymap.map'", your root folder will likely be 'C:/MyGame/'.
			                                         If '-map' is relative to the game directory (e.g. "-map 'games/maps/mymap.map'"), I will instead
			                                         use the current directory as the root.
			                                         
			                         -debugfreeze:   Freezes the compiler until a debugger is attached. Of no use to you, unless you're a developer.
			                                         E.g. '-debugfreeze 1' will freeze the compiler until a debugger is attached. By default, it is off.
			                       """;

			// Since this is currently only ever called before the engine is initialized, we must use System.Console.
			System.Console.WriteLine( message );
		}
	}
}
