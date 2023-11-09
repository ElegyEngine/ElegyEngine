// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets
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
