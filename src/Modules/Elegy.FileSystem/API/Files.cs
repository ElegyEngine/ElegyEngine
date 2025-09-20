// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces.Services;
using Elegy.Common.Utilities;

namespace Elegy.FileSystem.API
{
	/// <summary>
	/// Elegy filesystem interface.
	/// </summary>
	public static partial class Files
	{
		public static bool Init( in LaunchConfig config )
		{
			mLogger.Log( "Init" );

			mEngineConfig = config.Engine;

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

		public static void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			mOtherGamePaths.Clear();
			mOtherGameConfigs.Clear();
		}
	}
}
