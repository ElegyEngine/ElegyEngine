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
using Elegy.Core;
using Elegy.FileSystem.API;
using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;
using Elegy.MapCompiler.Processors;

namespace Elegy.MapCompiler
{
	public static partial class Program
	{
		private static MapCompilerParameters mParameters = new();
		private static TaggedLogger mLogger = new( "MapCompiler" );

		public static void Main( string[] args )
		{
			if ( !ProcessArgs( args ) )
			{
				PrintUsage();
				Environment.ExitCode = 1;
				return;
			}

			if ( mParameters.DebugFreeze )
			{
				DebugFreeze();
			}

			if ( mParameters.WithoutGeometry && mParameters.WithoutVisibility && mParameters.WithoutLighting )
			{
				mLogger.Log( "You are using -nogeo, -novis and -nolight all at once." );
				mLogger.Log( "No work will be done on the map in this case." );
				mLogger.Log( "Just what on Earth do you expect? An eggless omelette?!" );
				return;
			}

			mLogger.Log( "Init" );

			if ( !VerifyAndFixPaths() )
			{
				mLogger.Error( "Failed to compile map: incorrect input paths" );
				Environment.ExitCode = 1;
				return;
			}

			LaunchConfig config = new()
			{
				Args = args,
				ToolMode = true
			};

			if ( !CoreTemplate.Run( config, CompileMap ) )
			{
				mLogger.Error( "Failed to compile map. Check the compile log above." );
				Environment.ExitCode = 1;
			}
		}

		private static void CompileMap()
		{
			BrushMapDocument? document = LoadBrushMap();
			if ( document is null )
			{
				return;
			}

			ElegyMapDocument? outputData;

			// TODO: GeometryProcessor could just be a static method that calls all these other ones
			// ProcessingData data = GeometryProcessor.Process( document, mParameters );
			if ( !mParameters.WithoutGeometry )
			{
				ProcessingData data = new();
				GeometryProcessor geometry = new( data, mParameters );
				geometry.GenerateGeometryFromMap( document );
				geometry.FixCoordinateSystem();
				geometry.FixBrushOrigins();
				geometry.Scale( mParameters.GlobalScale );
				geometry.UpdateBoundaries();
				//geometry.SmoothenNormals();
				//geometry.GenerateDualGrid();

				TransmissionProcessor transmission = new( data, mParameters );
				transmission.GenerateOutputData();
				transmission.OptimiseRenderSurfaces();
				transmission.LinkEmbeddedMeshes();

				outputData = transmission.GetOutputData();
			}
			else
			{
				outputData = AssetSystem.API.Assets.LoadLevel( mParameters.OutputPath );
				if ( outputData is null )
				{
					mLogger.Fatal( $"Cannot find '{mParameters.OutputPath}' for modification." );
					mLogger.Log( $"             ^ It is needed since you are using '-nogeo', meaning the" );
					mLogger.Log( $"             ^ input .map file is ignored; nothing is done with it." );
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
				LightProcessor light = new( outputData, mParameters );
				//light.GenerateLightmapUvs();
				//light.GenerateLightmapImages();
				//light.ProcessLighting();
			}

			AssetSystem.API.Assets.WriteLevel( $"{mParameters.RootPath}/{mParameters.OutputPath}", outputData );

			mLogger.Success(
				$"Map compiled successfully. The result can be found at '{mParameters.OutputPath}'." );
		}

		private static bool VerifyAndFixPaths()
		{
			if ( mParameters.MapFile == string.Empty )
			{
				// The engine is still uninitialized, so we must use System.Console instead.
				mLogger.Error( "No map file was provided. Specify one with '-map path/to/file.map'." );
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
						mLogger.Warning(
							$"No root folder specified: Guessing from the map file path that it is '{dir.FullName}'." );
					}
				}
				else
				{
					mParameters.RootPath = Directory.GetCurrentDirectory();

					mLogger.Log( "No root folder specified. Using the current working directory as the root:" );
					mLogger.Log( mParameters.RootPath );
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
				mParameters.OutputPath = Path.Join( Path.GetDirectoryName( mapPath ) ?? "",
					Path.ChangeExtension( mParameters.OutputPath, ".elf" ) );
			}

			BrushMapDocument? document;
			try
			{
				document = BrushMapDocument.FromValve220MapFile( mapPath );
			}
			catch ( Exception ex )
			{
				mLogger.Error( $"""
				                An error occured while parsing the map file!
				                Message: {ex.Message}.

				                An error while parsing the map means something went fundamentally wrong.
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
