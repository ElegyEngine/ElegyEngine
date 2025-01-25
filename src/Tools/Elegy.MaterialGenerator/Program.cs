// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Text;

namespace Elegy.MaterialGenerator
{
	public static class Program
	{
		public static void Main( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "Oh hey! I basically scan texture files and generate Elegy materials for them. :)" );
				Console.WriteLine( "In order to use me, provide a path to your game folder, for example:" );
				Console.WriteLine( "./Elegy.MaterialGenerator \"C:/MyGame/game\"" );
				return;
			}

			if ( !Directory.Exists( args[0] ) )
			{
				Console.WriteLine( "The path you provided:" );
				Console.WriteLine( $"'{args[0]}'" );
				Console.WriteLine( "Does not exist!" );
				return;
			}

			string rootPath = args[0].Replace( '\\', '/' );
			string materialsPath = Path.Combine( rootPath, "materials" ).Replace( '\\', '/' );
			string texturesPath = Path.Combine( rootPath, "textures" ).Replace( '\\', '/' );

			if ( !ValidatePath( rootPath, materialsPath ) )
			{
				return;
			}

			if ( !ValidatePath( rootPath, texturesPath ) )
			{
				return;
			}

			do
			{
				var entries = Directory.GetFileSystemEntries( texturesPath, "*", SearchOption.AllDirectories );
				foreach ( var entry in entries )
				{
					if ( !Directory.Exists( entry ) )
					{
						continue;
					}

					string textureDirectory = Path.GetRelativePath( texturesPath, entry ).Replace( '\\', '/' );
					if ( textureDirectory.StartsWith( "tools" ) )
					{
						Console.WriteLine(
							$"Skipping '{textureDirectory}' because it contains tool textures - you gotta do those manually." );
						continue;
					}

					StringBuilder sb = new();
					Console.WriteLine( $"Processing textures for: {textureDirectory}" );

					var imageEntries = Directory.GetFiles( Path.Combine( texturesPath, textureDirectory ), "*" );
					foreach ( var imageEntry in imageEntries )
					{
						string imagePath = Path.GetRelativePath( entry, imageEntry );
						string imagePathNoExt = Path.ChangeExtension( imagePath, null );
						Console.WriteLine( $"  * {imagePathNoExt}" );
						sb.AppendLine(
							$$"""
							  materials/{{textureDirectory}}/{{imagePathNoExt}}
							  {
							  	materialTemplate Standard
							  	{
							  		map textures/{{textureDirectory}}/{{imagePath}}
							  	}
							  }

							  """ );
					}

					string materialFileName = textureDirectory
						.Replace( '/', '_' )
						.Replace( ' ', '_' );

					File.WriteAllText( $"{materialsPath}/mat_{materialFileName}.shader", sb.ToString() );

					Console.WriteLine( " - done!" );
				}

				Console.WriteLine( "Done! Press any key to repeat all this, or press ESC if you're done." );
			} while ( Console.ReadKey().Key != ConsoleKey.Escape );
		}

		public static bool ValidatePath( string root, string path )
		{
			if ( Directory.Exists( path ) )
			{
				return true;
			}

			Console.WriteLine( "ERROR: The following path doesn't exist:" );
			Console.WriteLine( $"'{path}'" );

			foreach ( var file in Directory.GetFiles( root ) )
			{
				if ( Path.GetExtension( file ) != ".exe" )
				{
					continue;
				}
				
				string filename = Path.GetRelativePath( root, file );
				
				if ( filename.Contains( "Elegy" ) || filename.Contains( "Launcher" ) )
				{
					Console.WriteLine( $"One or more engine launchers detected ({filename})." );
					Console.WriteLine( "This means you are in the game's root directory." );
					Console.WriteLine( "You most likely need to point to a subfolder from here, such as:" );
					foreach ( var folder in Directory.GetDirectories( root ) )
					{
						string folderRelative = Path.GetRelativePath( root, folder );
						if ( folderRelative is "engine" or "runtimes" )
						{
							continue;
						}
						
						Console.WriteLine( $"  * {folderRelative}" );
					}

					break;
				}
			}

			return false;
		}
	}
}
