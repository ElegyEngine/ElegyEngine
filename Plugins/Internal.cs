// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class Plugins
	{
		internal static void SetPluginSystem( PluginSystem pluginSystem )
		{
			mPluginSystem = pluginSystem;
		}

		private static PluginSystem mPluginSystem;
	}
}
