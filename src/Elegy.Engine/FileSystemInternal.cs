﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.API;

namespace Elegy.Engine
{
	internal sealed class FileSystemInternal
	{
		private TaggedLogger mLogger = new( "FileSystem" );

		public FileSystemInternal( EngineConfig engineConfig )
		{
			FileSystem.SetFileSystem( this );
			mEngineConfig = engineConfig;
		}

		public bool Init()
		{
			mLogger.Log( "Init" );
			
			if ( !Mount( mEngineConfig.EngineFolder, mountOthers: false, isBase: false, isEngine: true ) )
			{
				mLogger.Error( $"Failed to mount '{mEngineConfig.EngineFolder}'" );
				return false;
			}

			if ( !Mount( mEngineConfig.BaseFolder, mountOthers: true, isBase: true ) )
			{
				mLogger.Error( $"Failed to mount '{mEngineConfig.BaseFolder}'" );
				return false;
			}

			return true;
		}

		public void Shutdown()
		{
			mOtherGamePaths.Clear();
			mOtherGameConfigs.Clear();
		}

		public bool Mount( string directory, bool mountOthers = false, bool isBase = false, bool isEngine = false )
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

		public bool Unmount( string directory )
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

		public ApplicationConfig CurrentApplicationConfig => mBaseGameConfig;

		public IReadOnlyList<ApplicationConfig> Configs => mOtherGameConfigs;

		public EngineConfig EngineConfig => mEngineConfig;

		public bool Exists( string path, PathFlags flags = PathFlags.All, bool excludeOtherMountedDirectories = false, bool onlyRootPath = false )
			=> PathTo( path, flags, excludeOtherMountedDirectories, onlyRootPath ) != null;

		public string? PathTo( string destination, PathFlags flags = PathFlags.All, bool excludeOtherMountedDirectories = false, bool onlyRootPath = false )
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
			string withBasePath = $"{BaseGamePath}/{destination}";
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

		private bool ExistsDirect( string destination, PathFlags flags )
		{
			if ( flags.HasFlag( PathFlags.File ) && File.Exists( $"{mRootPath}/{destination}" ) )
			{
				return true;
			}

			if ( flags.HasFlag( PathFlags.Directory ) && Directory.Exists( $"{mRootPath}/{destination}" ) )
			{
				return true;
			}

			return false;
		}

		internal string[]? GetFileSystemEntries( string directory, string searchPattern, PathFlags flags, bool recursive )
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
			checkThenAdd( $"{BaseGamePath}/{directory}" );
			
			// 3. Mounted paths
			for ( int i = 0; i < mOtherGamePaths.Count; i++ )
			{
				checkThenAdd( $"{mOtherGamePaths[i]}/{directory}" );
			}

			// 4. Engine
			checkThenAdd( $"{EnginePath}/{directory}" );

			return entries.ToArray();
		}

		private string BaseGamePath => mEngineConfig.BaseFolder;
		private string EnginePath => mEngineConfig.EngineFolder;

		private string mRootPath = Directory.GetCurrentDirectory();
		private List<string> mOtherGamePaths = new();

		private ApplicationConfig mBaseGameConfig = default( ApplicationConfig );
		private List<ApplicationConfig> mOtherGameConfigs = new();
		private EngineConfig mEngineConfig = default( EngineConfig );
	}
}
