// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.MapCompiler.ConsoleArguments;

namespace Elegy.MapCompiler
{
	public static partial class Program
	{
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
			                       > Elegy.MapCompiler -map "game/maps/mymap.map" -root "C:/MyGame"

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

			mLogger.Warning( message );
		}
	}
}
