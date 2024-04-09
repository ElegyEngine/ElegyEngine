// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// TODO: We have mounted paths, however we will also need addon paths
// Currently, the order of searching goes like this:
// 1. Absolute paths
// 2. Current game directory
// 3. Mounted game directory
// 4. Engine directory
// Addons would insert themselves between 1 and 2, essentially overriding the game directory.

using Elegy.Common.Assets;

namespace Elegy.FileSystem.API
{
	/// <summary>
	/// Elegy filesystem path flags.
	/// </summary>
	[Flags]
	public enum PathFlags
	{
		/// <summary>
		/// This path is a file.
		/// </summary>
		File = 1,

		/// <summary>
		/// This path is a directory.
		/// </summary>
		Directory = 2,

		/// <summary>
		/// Flag for all and any kinds of paths, whether files or directories.
		/// </summary>
		All = File | Directory
	}

	/// <summary>
	/// Elegy filesystem interface.
	/// </summary>
	public static partial class Files
	{
		/// <summary>
		/// Mounts an application directory. Looks for an "applicationConfig.json" inside this directory,
		/// parses it and optionally mounts other directories referenced by the config.
		/// </summary>
		/// <param name="directory">The directory to mount.</param>
		/// <param name="mountOthers">Mount other dirs referenced by the config?</param>
		/// <returns>True on success, false if the directory does not exist, the config doesn't exist and so on.</returns>
		public static bool Mount( string directory, bool mountOthers = false, bool isBase = false, bool isEngine = false )
		{
			if ( !ExistsDirect( directory, PathFlags.Directory ) )
			{
				mLogger.Warning( $"Tried mounting '{directory}' but it does not exist" );
				return false;
			}

			mLogger.Log( $"Mounting '{directory}'..." );
			if ( mOtherGamePaths.Exists( path => path == directory ) )
			{
				mLogger.Log( "...already exists!" );
				return true;
			}

			// The engine folder does not require a config
			// The engine config is stored in the root directory
			if ( !isEngine )
			{
				string configPath = $"{directory}/applicationConfig.json";
				if ( !Exists( configPath ) )
				{
					mLogger.Error( $"Couldn't find '{directory}/applicationConfig.json'" );
					return false;
				}

				ApplicationConfig gameConfig = new();
				if ( !Common.Text.JsonHelpers.LoadFrom( ref gameConfig, $"{directory}/applicationConfig.json" ) )
				{
					mLogger.Error( $"Failed to parse '{directory}/applicationConfig.json'" );
					return false;
				}

				if ( isBase )
				{
					mBaseGameConfig = gameConfig;
					mLogger.Log( "Mounted as base directory" );
				}
				else
				{
					mOtherGameConfigs.Add( gameConfig );
					mOtherGamePaths.Add( directory );
					mLogger.Log( $"Mounted directory '{directory}'" );
				}

				// Mount other directories that this app depends on
				if ( mountOthers )
				{
					foreach ( string otherPath in gameConfig.Mounts )
					{
						if ( !Mount( otherPath, false ) )
						{
							mLogger.Warning( $"Can't mount dependency '{otherPath}', you may have missing content!" );
						}
					}
				}
			}
			else
			{
				mLogger.Log( "Mounted as engine directory" );
			}

			return true;
		}

		/// <summary>
		/// Unmounts a directory.
		/// </summary>
		/// <param name="directory"></param>
		/// <returns>True if successfully unmounted, false if the directory was already not mounted.</returns>
		public static bool Unmount( string directory )
		{
			// This is a bit of a hack, must do this properly
			// Maybe use a dictionary for paths & configs
			int index = mOtherGamePaths.IndexOf( directory );
			if ( index < 0 )
			{
				mLogger.Warning( $"Can't unmount '{directory}', it isn't mounted" );
				return false;
			}

			mOtherGamePaths.RemoveAt( index );
			mOtherGameConfigs.RemoveAt( index );
			mLogger.Warning( $"Unmounted '{directory}'" );

			return true;
		}

		/// <summary>
		/// Current application config.
		/// </summary>
		public static ApplicationConfig CurrentConfig => mBaseGameConfig;

		/// <summary>
		/// All application configs except the current one.
		/// </summary>
		public static IReadOnlyList<ApplicationConfig> Configs => mOtherGameConfigs;

		/// <summary>
		/// The current engine configuration.
		/// </summary>
		public static EngineConfig EngineConfig => mEngineConfig;

		/// <summary>
		/// Directory containing the current application/game's assets.
		/// </summary>
		public static string CurrentGamePath => EngineConfig.BaseFolder;

		/// <summary>
		/// Directory containing the engine's base assets.
		/// </summary>
		public static string EnginePath => EngineConfig.EngineFolder;

		/// <param name="path">The relative path to be checked.</param>
		/// <param name="flags">Filter flags.</param>
		/// <param name="excludeOtherMountedDirectories">If true, only search in engine dir and base dir.</param>
		/// <returns>True if the path exists and passes the filter flags.</returns>
		public static bool Exists( string path, PathFlags flags = PathFlags.All, bool excludeOtherMountedDirectories = false, bool onlyRootPath = false )
			=> PathTo( path, flags, excludeOtherMountedDirectories, onlyRootPath ) != null;

		/// <param name="destination">Relative path to the destination.</param>
		/// <param name="flags">Filter flags.</param>
		/// <param name="excludeOtherMountedDirectories">If true, only search in engine dir and base dir.</param>
		/// <returns>Working-directory-relative path to the destination in one of the mounted dirs.</returns>
		public static string? PathTo( string destination, PathFlags flags = PathFlags.All, bool excludeOtherMountedDirectories = false, bool onlyRootPath = false )
		{
			// Absolute paths take 1st priority
			if ( ExistsDirect( destination, flags ) )
			{
				return destination;
			}

			if ( onlyRootPath )
			{
				return null;
			}

			// Current game dir is 2nd priority
			string withBasePath = $"{CurrentGamePath}/{destination}";
			if ( ExistsDirect( withBasePath, flags ) )
			{
				return withBasePath;
			}

			// Other mounted directories & engine are checked last, as they are considered fallbacks
			if ( !excludeOtherMountedDirectories )
			{
				foreach ( string otherPath in mOtherGamePaths )
				{
					string withOtherPath = $"{otherPath}/{destination}";
					if ( ExistsDirect( withOtherPath, flags ) )
					{
						return withOtherPath;
					}
				}
			}

			string withEnginePath = $"{EnginePath}/{destination}";
			if ( ExistsDirect( withEnginePath, flags ) )
			{
				return withEnginePath;
			}

			return null;
		}

		/// <param name="directory">A directory such as <c>"maps"</c> or <c>"models/vegetation"</c>.</param>
		/// <param name="searchPattern">The search pattern to filter out filenames.</param>
		/// <param name="recursive">Whether or not to scan subfolders too.</param>
		/// <returns>A list of files relative to <paramref name="directory"/>,
		///  <c>null</c> if <paramref name="directory"/> doesn't exist in any mounted path.</returns>
		public static string[]? GetFiles( string directory, string searchPattern = "*", bool recursive = false )
			=> GetEntries( directory, searchPattern, PathFlags.File, recursive );

		/// <param name="directory">A directory such as <c>"maps"</c> or <c>"models/vegetation"</c>.</param>
		/// <param name="searchPattern">The search pattern to filter out directory names.</param>
		/// <param name="recursive">Whether or not to scan subfolders too.</param>
		/// <returns>A list of directories relative to <paramref name="directory"/>,
		///  <c>null</c> if <paramref name="directory"/> doesn't exist in any mounted path.</returns>
		public static string[]? GetDirectories( string directory, string searchPattern = "*", bool recursive = false )
			=> GetEntries( directory, searchPattern, PathFlags.Directory, recursive );

		/// <summary>
		/// Similar to <see cref="GetFiles"/> and <see cref="GetDirectories"/>, except it can include either filesystem entries.
		/// Useful for implementing your own <c>"ls"</c> or <c>"dir"</c> console command.
		/// </summary>
		public static string[]? GetEntries( string directory, string searchPattern = "*", PathFlags flags = PathFlags.All, bool recursive = false )
		{
			if ( !Exists( directory, PathFlags.Directory ) )
			{
				return null;
			}

			SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			List<string> entries = new();

			var checkThenAdd = ( string path ) =>
			{
				if ( ExistsDirect( path, PathFlags.Directory ) )
				{
					string[] systemEntries = flags switch
					{
						PathFlags.All => Directory.GetFileSystemEntries( $"{mRootPath}/{path}", searchPattern, searchOption ),
						PathFlags.Directory => Directory.GetDirectories( $"{mRootPath}/{path}", searchPattern, searchOption ),
						PathFlags.File => Directory.GetFiles( $"{mRootPath}/{path}", searchPattern, searchOption ),
						_ => Array.Empty<string>()
					};

					for ( int i = 0; i < systemEntries.Length; i++ )
					{
						systemEntries[i] = systemEntries[i].Replace( '\\', '/' );
					}

					entries.AddRange( systemEntries );
				}
			};

			// 1. Absolute path
			checkThenAdd( directory );

			// TODO: Addon paths between 1 and 2

			// 2. Current game path
			checkThenAdd( $"{CurrentGamePath}/{directory}" );

			// 3. Mounted paths
			for ( int i = 0; i < mOtherGamePaths.Count; i++ )
			{
				checkThenAdd( $"{mOtherGamePaths[i]}/{directory}" );
			}

			// 4. Engine
			checkThenAdd( $"{EnginePath}/{directory}" );

			return entries.ToArray();
		}
	}
}
