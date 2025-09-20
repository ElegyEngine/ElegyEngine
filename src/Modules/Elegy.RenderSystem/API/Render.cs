// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using System.Diagnostics;

namespace Elegy.RenderSystem.API
{
	/// <summary>
	/// The rendering system.
	/// </summary>
	public static partial class Render
	{
		private static string[] mAdditionalInstanceExtensions = [];
		private static string[] mAdditionalDeviceExtensions = [];
		private static bool mInitialised;

		public static bool Init( LaunchConfig config )
		{
			mLogger.Log( "Init" );

			mAdditionalInstanceExtensions = config.VulkanInstanceExtensions;
			mAdditionalDeviceExtensions = config.VulkanDeviceExtensions;

			mStopwatch = Stopwatch.StartNew();
			mInitialised = true;

			return true;
		}

		public static bool CreateGraphicsDevice( LaunchConfig config )
		{
			if ( !InitialiseGraphicsDevice() )
			{
				mLogger.Error( "Failed to create graphics device" );
				return false;
			}

			// Builtin samplers, layouts etc.
			InitialiseGraphicsConstants();
			return true;
		}

		public static bool CreateGraphics( LaunchConfig config )
		{
			if ( RenderStyle is null )
			{
				mLogger.Error( "No render style loaded, dunno how to render things without that" );
				return false;
			}

			// Create pipelines etc. from shader templates
			if ( !InitialiseGraphics() )
			{
				mLogger.Error( "Failed to initialise graphics, can't render anything without that" );
				return false;
			}

			// Create fullscreen quad etc.
			InitialiseBuiltinMeshes();

			return RenderStyle.CreateCorePipelines();
		}

		public static void Shutdown()
		{
			if ( !mInitialised )
			{
				return;
			}
			
			mLogger.Log( "Shutdown" );
			mInitialised = false;
		}
	}
}
