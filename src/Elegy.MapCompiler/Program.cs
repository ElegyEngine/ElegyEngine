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
using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.ConsoleArguments;
using Elegy.MapCompiler.Data.Processing;
using Elegy.MapCompiler.Processors;

namespace Elegy.MapCompiler
{
	public static partial class Program
	{
		private static MapCompilerParameters mParameters = new();

		public static void Main( string[] args )
		{
			Console.WriteLine( $"### Elegy.MapCompiler - built 2023/11/08 ###" );
			Console.WriteLine( "I eat TrenchBroom and J.A.C.K. maps and produce compiled Elegy levels." );
			Console.WriteLine( "Let's keep the fire... burning." );
			Console.WriteLine();

			if ( !ProcessArgs( args ) )
			{
				PrintUsage();
				return;
			}

			if ( mParameters.DebugFreeze > 0.0f )
			{
				DebugFreeze( mParameters.DebugFreeze );
			}

			if ( mParameters.OutputPath == string.Empty )
			{
				mParameters.OutputPath = Path.GetFileNameWithoutExtension( mParameters.MapFile );
			}

			if ( mParameters.MapFile == string.Empty )
			{
				Console.WriteLine( "No map file was provided." );
				return;
			}

			if ( mParameters.GameDirectory == string.Empty )
			{
				Console.WriteLine( "Hey uh, you gotta give me the game directory too." );
				Console.WriteLine( "E.g. -gamedir \"C:/Games/MyGame/base\"" );
				return;
			}

			if ( !FileSystem.Init( mParameters.GameDirectory ) )
			{
				Console.WriteLine( "Since the file system failed to initialise, I cannot keep going. Sorry." );
				return;
			}

			MaterialSystem.Init();

			string mapPath = FileSystem.GetPathTo( mParameters.MapFile );
			if ( !FileSystem.FileExists( mapPath ) )
			{
				Console.WriteLine();
				Console.WriteLine( "If I can't find the map, something's fundamentally wrong." );
				Console.WriteLine( "Double-check your paths, check for any typos, check the" );
				Console.WriteLine( "game directory etc. I am not sure what could be causing this." );
				return;
			}

			BrushMapDocument? document = null;
			try
			{
				document = BrushMapDocument.FromValve220MapFile( FileSystem.GetPathTo( mParameters.MapFile ) );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( "Error while parsing the map!" );
				Console.WriteLine( $"Message: {ex.Message}" );
				Console.WriteLine();
				Console.WriteLine( "An error while parsing the map means something went fundamentally wrong." );
				Console.WriteLine( "Open Notepad or something and check out that line & column, maybe try fixing it." );
				Console.WriteLine();
				Console.WriteLine( "If this is a perfectly valid .map file (is parsed fine in other compilers)," );
				Console.WriteLine( "and this still happens, then open an issue on my GitHub repository:" );
				Console.WriteLine( "    https://github.com/ElegyEngine/Elegy.MapCompiler/issues" );
				Console.WriteLine( "Thank you for your patience. :3" );
				return;
			}

			if ( document is null )
			{
				Console.WriteLine( "Unknown error occurred" );
				return;
			}

			ProcessingData data = new();
			GeometryProcessor processor = new( data, mParameters );
			processor.GenerateGeometryFromMap( document );
			processor.FixBrushOrigins();
			processor.UpdateBoundaries();
			//processor.SmoothenNormals();

			//VisibilityProcessor vp = new( data, mParameters );
			//vp.GenerateOctree();
			//vp.ProcessVisibility();

			//LightProcessor lp = new( data, mParameters );
			//lp.GenerateLightmapUvs();
			//lp.GenerateLightmapImages();
			//lp.ProcessLighting();

			OutputProcessor op = new( data, mParameters );
			op.GenerateOutputData();
			op.WriteToFile( FileSystem.GetPathTo( $"{mParameters.OutputPath}.elf" ) );

			Console.WriteLine( "Compilation's done. You can try out your map now!" );
		}

		private static void DebugFreeze( float seconds )
		{
			Console.WriteLine( "DEVELOPER: You have entered the 'debug freeze'. The purpose of this is to freeze" );
			Console.WriteLine( $"           the app so you can attach a debugger for about {(int)seconds} seconds." );
			Console.WriteLine( "           Now go ahead. Attach the debugger while you still have the time." );

			int quarterSecondCounter = (int)seconds * 4;
			while ( quarterSecondCounter >= 0 )
			{
				Thread.Sleep( 250 );

				if ( System.Diagnostics.Debugger.IsAttached )
				{
					Console.WriteLine();
					Console.WriteLine( "Alrighty, you've attached it! Let's go now." );
					Console.WriteLine();
					Thread.Sleep( 500 );
					return;
				}
			}

			Console.WriteLine( "Welp, no debugger was attached, moving on!" );
			Console.WriteLine();
		}

		private static bool ProcessArgs( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "No arguments were provided." );
				return false;
			}

			if ( args.Length == 1 && args[0] == "-help" )
			{
				Console.WriteLine( "Ah, a lost soul like myself. Let me guide you..." );
				return false;
			}

			ParameterManager.ProcessArguments( args, out mParameters );
			return true;
		}

		private static void PrintUsage()
		{
			Console.WriteLine( "In order to compile a map, you need to call me like so:" );
			Console.WriteLine( "> Elegy.MapCompiler -map \"maps/mymap.map\" -gamedirectory \"C:/MyGame/game\"" );
			Console.WriteLine();
			Console.WriteLine( "Now, here's a list of all parameters:" );
			Console.WriteLine();
			Console.WriteLine( "| NAME          | DESCRIPTION                                                          |" );
			Console.WriteLine();
			Console.WriteLine( " -map:           Path to the map, preferably absolute, can be relative to the game directory." );
			Console.WriteLine();
			Console.WriteLine( " -out:           Output name, relative to the map file or absolute." );
			Console.WriteLine();
			Console.WriteLine( " -gamedirectory: Path to the game directory, from which all assets are pulled." );
			Console.WriteLine( "                 If left empty, I'll try to guess the game directory from the provided path." );
			Console.WriteLine( "                 E.g. 'C:/MyGame/game/maps/mymap.map' is most likely gonna be 'C:/MyGame/game'." );
			Console.WriteLine( "                 If the map path is relative, oh well, I'll just use the current working directory." );
			Console.WriteLine( "                 Just keep in mind that that can sometimes be unreliable." );
			Console.WriteLine();
			Console.WriteLine( "This compiler is in a very early state, so this is all we have at the moment." );
		}
	}
}
