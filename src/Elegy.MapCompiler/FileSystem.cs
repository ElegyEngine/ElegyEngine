// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// TODO: replace with Elegy's actual FileSystem one day

namespace Elegy.MapCompiler
{
	public static class FileSystem
	{
		public static string GameDirectory { get; private set; } = string.Empty;

		public static bool Init( string gameDirectory )
		{
			GameDirectory = gameDirectory
				.Replace( '\\', '/' )
				.TrimEnd( '/' );

			if ( !Path.IsPathRooted( GameDirectory ) )
			{
				Console.WriteLine( "[FileSystem] Relative gamedir detected, we're in for some smooth sailing then. :)" );
				GameDirectory = $"{Directory.GetCurrentDirectory()}/{GameDirectory}".Replace( '\\', '/' );
			}

			if ( !Directory.Exists( GameDirectory ) )
			{
				Console.WriteLine( "[FileSystem] Hey uh... the gamedir you gave me doesn't seem to be right." );
				Console.WriteLine( "             It either points to a folder that doesn't exist, or it's pointing" );
				Console.WriteLine( "             to a file. I don't have enough technology to determine what exactly yet." );
				Console.WriteLine( "             Either way, please fix this, else I can't compile the map." );
				return false;
			}

			return true;
		}

		public static bool FileExists( string path )
		{
			if ( File.Exists( path ) )
			{
				return true;
			}

			return File.Exists( $"{GameDirectory}/{path}" );
		}

		public static bool DirectoryExists( string path )
		{
			if ( Directory.Exists( path ) )
			{
				return true;
			}

			return Directory.Exists( $"{GameDirectory}/{path}" );
		}

		public static byte[] ReadAllBytes( string path )
		{
			if ( File.Exists( path ) )
			{
				return File.ReadAllBytes( path );
			}

			return File.ReadAllBytes( $"{GameDirectory}/{path}" );
		}

		public static string GetPathTo( string path )
		{
			if ( Path.IsPathRooted( path ) )
			{
				return path;
			}

			return $"{GameDirectory}/{path}";
		}
	}
}
