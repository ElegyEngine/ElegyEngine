// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Interfaces;

namespace Elegy.Core
{
	internal class AssetPluginCollector : IPluginCollector
	{
		public void OnPluginLoaded( IPlugin plugin )
			=> Assets.RegisterLoader( plugin );

		public void OnPluginUnloaded( IPlugin plugin )
			=> Assets.UnregisterLoader( plugin );
	}
}
