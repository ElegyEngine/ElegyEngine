// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.AssetSystem.Interfaces;

namespace Elegy.AssetSystem.API
{
	public static partial class Assets
	{
		/// <summary>
		/// Loads a level from the path.
		/// </summary>
		public static ElegyMapDocument? LoadLevel( string path )
		{
			string? fullPath = mFileSystem.PathToFile( path );
			if ( fullPath is null )
			{
				mLogger.Error( $"LoadLevel: Can't find level '{path}'" );
				return null;
			}

			string extension = Path.GetExtension( path ) ?? "";
			ILevelLoader? levelLoader = FindLevelLoader( extension );
			if ( levelLoader is null )
			{
				mLogger.Error( $"LoadLevel: Unsupported format '{extension}'" );
				return null;
			}

			ElegyMapDocument? level = levelLoader.LoadLevel( fullPath );
			if ( level is null )
			{
				mLogger.Error( $"LoadLevel: Cannot load level '{path}'\nFull path: {fullPath}" );
				return null;
			}

			mLevels[path] = level;
			return level;
		}

		/// <summary>
		/// Writes a level to the given path.
		/// </summary>
		public static bool WriteLevel( string path, ElegyMapDocument data )
		{
			ILevelWriter? writer = FindLevelWriter( Path.GetExtension( path ) );
			if ( writer is null )
			{
				mLogger.Error( $"WriteLevel: Unsupported level extension: '{path}'" );
				return false;
			}

			return writer.WriteLevel( path, data );
		}

		/// <summary>
		/// Finds an appropriate <see cref="ILevelLoader"/> according to one of the <paramref name="extensions"/>.
		/// </summary>
		public static ILevelLoader? FindLevelLoader( params string[] extensions )
		{
			foreach ( var levelLoader in mLevelLoaders )
			{
				for ( int i = 0; i < extensions.Length; i++ )
				{
					if ( levelLoader.Supports( extensions[i] ) )
					{
						return levelLoader;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Finds an appropriate <see cref="ILevelWriter"/> according to the <paramref name="extension"/>.
		/// </summary>
		public static ILevelWriter? FindLevelWriter( string extension )
		{
			foreach ( var levelWriter in mLevelWriters )
			{
				if ( levelWriter.Supports( extension ) )
				{
					return levelWriter;
				}
			}

			return null;
		}

		/// <summary>
		/// A collection of all loaded levels.
		/// </summary>
		public static IReadOnlyCollection<ElegyMapDocument> AllLevels => mLevels.Values;
	}
}
