// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.PluginSystem.API;
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
		
		public static bool Init( in LaunchConfig config )
		{
			mLogger.Log( "Init" );

			mAdditionalInstanceExtensions = config.VulkanInstanceExtensions;
			mAdditionalDeviceExtensions = config.VulkanDeviceExtensions;

			Plugins.RegisterDependency( "Elegy.RenderBackend", typeof( RenderBackend.Utils ).Assembly );
			Plugins.RegisterDependency( "Elegy.RenderSystem", typeof( Render ).Assembly );
			Plugins.RegisterPluginCollector( new RenderPluginCollector() );

			mStopwatch = Stopwatch.StartNew();

			return true;
		}

		public static bool PostInit()
		{
			if ( !InitialiseGraphicsDevice() )
			{
				mLogger.Error( "Failed to create graphics device" );
				return false;
			}

			// Builtin samplers, layouts etc.
			InitialiseGraphicsConstants();

			// Load material and shader templates
			if ( !LoadMaterialTemplates() )
			{
				return false;
			}

			Assets.SetRenderFactories(
				CreateMaterial,
				( textureInfo, data ) => CreateTexture( textureInfo, data.AsSpan() ) );

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
			mLogger.Log( "Shutdown" );

			Plugins.UnregisterPluginCollector<RenderPluginCollector>();
			Plugins.UnregisterDependency( "Elegy.RenderSystem" );
			Plugins.UnregisterDependency( "Elegy.RenderBackend" );
		}
	}
}
