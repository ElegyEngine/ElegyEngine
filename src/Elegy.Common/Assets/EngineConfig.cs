// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets
{
	/// <summary></summary>
	public struct EngineConfig
	{
		/// <summary></summary>
		public EngineConfig()
		{

		}

		/// <summary></summary>
		public string? ConfigName { get; set; } = null;
		/// <summary></summary>
		public string EngineFolder { get; set; } = "engine";
		/// <summary></summary>
		public string BaseFolder { get; set; } = "game";
		/// <summary></summary>
		public string[] EnginePlugins { get; set; } = Array.Empty<string>();
	}
}
