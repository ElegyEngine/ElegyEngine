// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.PluginSystem.API;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		public static bool Init( in LaunchConfig config )
		{
			mLogger.Log( "Init" );
			Plugins.RegisterDependency( "Elegy.RenderBackend", typeof( RenderBackend.Utils ).Assembly );
			Plugins.RegisterDependency( "Elegy.RenderSystem", typeof( Render ).Assembly );
			Plugins.RegisterPluginCollector( new RenderPluginCollector() );

			return true;
		}

		public static bool PostInit()
		{
			if ( !LoadMaterialTemplates() )
			{
				return false;
			}

			return Instance.CreateCorePipelines();
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
