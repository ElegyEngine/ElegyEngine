// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

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
	}
}
