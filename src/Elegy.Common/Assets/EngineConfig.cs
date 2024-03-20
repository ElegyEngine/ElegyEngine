// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets
{
	public struct EngineConfig
	{
		public EngineConfig()
		{

		}

		public string? ConfigName { get; set; } = null;
		public string EngineFolder { get; set; } = "engine";
		public string BaseFolder { get; set; } = "game";
		public string[] EnginePlugins { get; set; } = Array.Empty<string>();
	}
}
