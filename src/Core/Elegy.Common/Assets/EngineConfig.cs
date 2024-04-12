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
		public string? ConfigName { get; set; } = "Default";
		/// <summary></summary>
		public string EngineFolder { get; set; } = "engine";
		/// <summary></summary>
		public string BaseFolder { get; set; } = "game";
		/// <summary></summary>
		public string[] EnginePlugins { get; set; } =
		[
			"plugins/RenderStandard",
			// We don't have audio yet, so this one shall wait
			//"plugins/AudioStandard",
			"plugins/DevConsole"
		];
	}
}
