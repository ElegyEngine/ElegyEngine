// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.Bootstrapper
{
	public static partial class ElegyApplication
	{
		public static void Validate( string region, in LaunchConfig config, Func<LaunchConfig, bool> initialiser )
		{
			if ( !initialiser( config ) )
			{
				throw new Exception( region );
			}
		}

		static partial void Init_Generated( in LaunchConfig config );
		static partial void Shutdown_Generated();
		public static void Run( in LaunchConfig config, Action loop )
		{
			try
			{
				Init_Generated( config );
			}
			catch ( Exception ex )
			{
				Console.Error.WriteLine( $"ERROR: couldn't initialise {ex.Message}" );
				Shutdown_Generated();
				return;
			}

			loop();
			Shutdown_Generated();
		}

		[ElegyMain]
		[WithAssetSystem]
		public static void Main( string[] args )
			=> ElegyApplication.Run( 
			config: new()
			{
				Args = args,
				Engine = new()
				{
					BaseFolder = "game"
				}
			},

			loop: () =>
			{
				// update the engine, render frames etc.
			} );
	}
}
