// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets
{
	public struct PluginConfig
	{
		public string AssemblyName { get; set; }
		public string Description { get; set; }
		public string Author { get; set; }
		public string VersionDateString { get; set; }
		public string EngineVersion { get; set; }
		public string ImplementedInterface { get; set; }
	}
}
