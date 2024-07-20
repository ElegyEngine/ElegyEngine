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

			if ( mParameters.WithoutGeometry && mParameters.WithoutVisibility && mParameters.WithoutLighting )
			{
				System.Console.WriteLine( "You are using -nogeo, -novis and -nolight all at once." );
				System.Console.WriteLine( "No work will be done on the map in this case." );
				System.Console.WriteLine( "Just what on Earth do you expect? An eggless omelette?!" );
				return;
			}

			System.Console.WriteLine( "Init" );

			if ( !VerifyAndFixPaths() )
			{
				System.Console.WriteLine( "Failed to compile map: incorrect input paths" );
				return;
			}

			if ( !LaunchEngine() )
			{
				System.Console.WriteLine( "Failed to compile map: cannot launch engine" );
				return;
			}

			BrushMapDocument? document = LoadBrushMap();
			if ( document is null )
			{
				return;
			}

			ElegyMapDocument? outputData = null;

			// TODO: GeometryProcessor could just be a static method that calls all these other ones
			// ProcessingData data = GeometryProcessor.Process( document, mParameters );
			if ( !mParameters.WithoutGeometry )
			{
				ProcessingData data = new();
				GeometryProcessor geometry = new( data, mParameters );
				geometry.GenerateGeometryFromMap( document );
				geometry.Scale( mParameters.GlobalScale );
				geometry.FixCoordinateSystem();
				geometry.FixBrushOrigins();
				geometry.UpdateBoundaries();
				//geometry.SmoothenNormals();
				//geometry.GenerateDualGrid();

				TransmissionProcessor transmission = new( data, mParameters );
				transmission.GenerateOutputData();
				transmission.OptimiseRenderSurfaces();

				outputData = transmission.GetOutputData();
			}
			else
			{
				outputData = AssetSystem.API.Assets.LoadLevel( mParameters.OutputPath );
				if ( outputData is null )
				{
					mLogger.Fatal( $"Cannot find '{mParameters.OutputPath}' for modification." );
					mLogger.Log( $"             ^ It is needed since you are using '-nogeo', meaning the input" );
					mLogger.Log( $"             ^ .map file is ignored; nothing is done with it." );
					return;
				}
			}

			// TODO: VisibilityProcessor.Process( outputData, mParameters );
			if ( !mParameters.WithoutVisibility )
			{
				VisibilityProcessor visibility = new( outputData, mParameters );
				//visibility.GenerateProbes();
				//visibility.FilterProbes();
				//visibility.ProcessVisibility();
			}

			// TODO: LightProcessor.Process( outputData, mParameters );
			if ( !mParameters.WithoutLighting )
			{
				LightProcessor lp = new( outputData, mParameters );
				//light.GenerateLightmapUvs();
				//light.GenerateLightmapImages();
				//light.ProcessLighting();
			}

			AssetSystem.API.Assets.WriteLevel( $"{mParameters.RootPath}/{mParameters.OutputPath}", outputData );

			mLogger.Success( $"Map compiled successfully. The result can be found at '{mParameters.OutputPath}'." );
		}

		private static bool LaunchEngine()
		{
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
				return false;
			}

			return true;
		}

		private static bool VerifyAndFixPaths()
		{
			if ( mParameters.MapFile == string.Empty )
			{
				// The engine is still uninitialized, so we must use System.Console instead.
				System.Console.WriteLine( "No map file was provided. Specify one with '-map path/to/file.map'." );
				return false;
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
					mParameters.RootPath = Directory.GetCurrentDirectory();

					System.Console.WriteLine( "No root folder specified. Using the current working directory as the root:" );
					System.Console.WriteLine( mParameters.RootPath );
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

			return true;
		}

		private static BrushMapDocument? LoadBrushMap()
		{
			// First obtain the full path to the .map file
			string? mapPath = Files.PathTo( mParameters.MapFile );
			if ( mapPath == null )
			{
				mLogger.Error( $"""
							   The specified map file '{mapPath}' does not exist or cannot be accessed.
							   Please check that the path is correct and that you are in the correct root folder. (Currently: '{Directory.GetCurrentDirectory()}')
							   You can specify a custom root folder with '-root path/to/root'.
							   """ );
				return null;
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
				return null;
			}

			if ( document is null )
			{
				mLogger.Fatal( "An unknown error occured." );
				return null;
			}

			return document;
		}
	}
}
