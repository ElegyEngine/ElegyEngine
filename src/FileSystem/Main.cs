// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

// TODO: We have mounted paths, however we will also need addon paths
// Currently, the order of searching goes like this:
// 1. Absolute paths
// 2. Current game directory
// 3. Mounted game directory
// 4. Engine directory
// Addons would insert themselves between 1 and 2, essentially overriding the game directory.

using Elegy.Assets;

namespace Elegy
{
	[Flags]
	public enum PathFlags
	{
		File = 1,
		Directory = 2,

		All = File | Directory
	}

	public static partial class FileSystem
	{
		/// <summary>
		/// Mounts an application directory. Looks for an "applicationConfig.json" inside this directory,
		/// parses it and optionally mounts other directories referenced by the config.
		/// </summary>
		/// <param name="directory">The directory to mount.</param>
		/// <param name="mountOthers">Mount other dirs referenced by the config?</param>
		/// <returns>True on success, false if the directory does not exist, the config doesn't exist and so on.</returns>
		public static bool Mount( string directory, bool mountOthers = false )
			=> mFileSystem.Mount( directory, mountOthers );

		/// <summary>
		/// Unmounts a directory.
		/// </summary>
		/// <param name="directory"></param>
		/// <returns>True if successfully unmounted, false if the directory was already not mounted.</returns>
		public static bool Unmount( string directory )
			=> mFileSystem.Unmount( directory );

		/// <summary>
		/// Current application config.
		/// </summary>
		public static ApplicationConfig CurrentConfig => mFileSystem.CurrentApplicationConfig;

		/// <summary>
		/// All application configs except the current one.
		/// </summary>
		public static IReadOnlyList<ApplicationConfig> Configs => mFileSystem.Configs;

		/// <summary>
		/// The current engine configuration.
		/// </summary>
		public static EngineConfig EngineConfig => mFileSystem.EngineConfig;

		/// <summary>
		/// Directory containing the current appliaction/game's assets.
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
		public static bool Exists( string path, PathFlags flags = PathFlags.All, bool excludeOtherMountedDirectories = false )
			=> mFileSystem.Exists( path, flags, excludeOtherMountedDirectories );

		/// <param name="destination">Relative path to the destination.</param>
		/// <param name="flags">Filter flags.</param>
		/// <param name="excludeOtherMountedDirectories">If true, only search in engine dir and base dir.</param>
		/// <returns>Working-directory-relative path to the destination in one of the mounted dirs.</returns>
		public static string? PathTo( string destination, PathFlags flags = PathFlags.All, bool excludeOtherMountedDirectories = false )
			=> mFileSystem.PathTo( destination, flags, excludeOtherMountedDirectories );

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
			=> mFileSystem.GetFileSystemEntries( directory, searchPattern, flags, recursive );
	}
}
