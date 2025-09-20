// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;

namespace Elegy.App
{
	internal class RenderPluginCollector : IPluginCollector
	{
		public void OnPluginLoaded( IPlugin plugin )
		{
			if ( plugin is IRenderStyle renderStyle )
			{
				Render.RenderStyle = renderStyle;
			}
		}

		public void OnPluginUnloaded( IPlugin plugin )
		{
			if ( plugin is not IRenderStyle renderStyle )
			{
				return;
			}

			if ( Render.RenderStyle == renderStyle )
			{
				Render.RenderStyle = null;
			}
		}
	}
}
