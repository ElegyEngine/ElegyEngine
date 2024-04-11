// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;

namespace Elegy.RenderSystem
{
	internal class RenderPluginCollector : IPluginCollector
	{
		public void OnPluginLoaded( IPlugin plugin )
		{
			if ( plugin is IRenderFrontend renderFrontend )
			{
				Render.SetFrontend( renderFrontend );
			}
		}

		public void OnPluginUnloaded( IPlugin plugin )
		{

		}
	}
}
