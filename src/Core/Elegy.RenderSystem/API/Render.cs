// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.PluginSystem.API;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		public static bool Init( in LaunchConfig config )
		{
			Plugins.RegisterDependency( "Elegy.RenderSystem", typeof( Render ).Assembly );
			Plugins.RegisterPluginCollector( new RenderPluginCollector() );

			return true;
		}
	}
}
