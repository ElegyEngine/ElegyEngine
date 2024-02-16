// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Text;

namespace Elegy.MaterialGenerator
{
	internal class Program
	{
		static void Main( string[] args )
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "Oh hey! I basically scan texture files and generate Elegy materials for them. :)" );
				Console.WriteLine( "In order to use me, provide a path to your textures folder, for example:" );
				Console.WriteLine( "C:/MyGame/game/materials/textures" );
				return;
			}

			if ( !Directory.Exists( args[0] ) )
			{
				Console.WriteLine( "The path you provided:" );
				Console.WriteLine( $"'{args[0]}'" );
				Console.WriteLine( "Does not exist!" );
				return;
			}

			do
			{
				string rootPath = args[0].Replace( '\\', '/' );

				var entries = Directory.GetFileSystemEntries( args[0], "*", SearchOption.AllDirectories );
				foreach ( var entry in entries )
				{
					if ( !Directory.Exists( entry ) )
					{
						continue;
					}

					string materialDirectoryRelative = Path.GetRelativePath( args[0], entry ).Replace( '\\', '/' );
					if ( materialDirectoryRelative.StartsWith( "tools" ) )
					{
						Console.WriteLine( $"Skipping '{materialDirectoryRelative}' because it contains tool textures - you gotta do those manually." );
						continue;
					}

					StringBuilder sb = new StringBuilder();
					Console.Write( $"Processing textures for: {materialDirectoryRelative}" );

					var imageEntries = Directory.GetFiles( Path.Combine( rootPath, materialDirectoryRelative ), "*" );
					foreach ( var imageEntry in imageEntries )
					{
						string imagePath = Path.ChangeExtension( Path.GetRelativePath( entry, imageEntry ), null );
						Console.WriteLine( $"  * {imagePath}" );
						sb.AppendLine(
							$$"""
							materials/{{materialDirectoryRelative}}/{{imagePath}}
							{
								materialTemplate Standard
								{
									map materials/textures/{{materialDirectoryRelative}}/{{imagePath}}
								}
							}

							""" );
					}

					string materialFileName = materialDirectoryRelative
						.Replace( '/', '_' )
						.Replace( ' ', '_' );

					File.WriteAllText( $"{args[0]}/../mat_{materialFileName}.shader", sb.ToString() );

					Console.WriteLine( " - done!" );
				}

				Console.WriteLine( "Done! Press any key to repeat all this, or press ESC if you're done." );
			} while ( Console.ReadKey().Key != ConsoleKey.Escape );
		}
	}
}
