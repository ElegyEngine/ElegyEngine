// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets
{
	/// <summary></summary>
	public struct PluginConfig
	{
		/// <summary></summary>
		public string AssemblyName { get; set; }
		/// <summary></summary>
		public string Description { get; set; }
		/// <summary></summary>
		public string Author { get; set; }
		/// <summary></summary>
		public string VersionDateString { get; set; }
		/// <summary></summary>
		public string EngineVersion { get; set; }
		/// <summary></summary>
		public string ImplementedInterface { get; set; }
	}
}
