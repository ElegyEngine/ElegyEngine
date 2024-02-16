// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Plugins
	{
		internal static void SetPluginSystem( PluginSystemInternal? pluginSystem )
			=> mPluginSystem = pluginSystem;

		private static PluginSystemInternal? mPluginSystem;
	}
}
