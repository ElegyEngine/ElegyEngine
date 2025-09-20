// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Utilities;

namespace Elegy.FileSystem.API
{
	public static partial class Files
	{
		private static TaggedLogger mLogger = new( "FileSystem" );

		private static string mRootPath = Directory.GetCurrentDirectory();
		private static List<string> mOtherGamePaths = new();

		private static ApplicationConfig mBaseGameConfig = default( ApplicationConfig );
		private static List<ApplicationConfig> mOtherGameConfigs = new();
		private static EngineConfig mEngineConfig = default( EngineConfig );

		private static bool ExistsDirect( string destination, PathFlags flags )
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
	}
}
