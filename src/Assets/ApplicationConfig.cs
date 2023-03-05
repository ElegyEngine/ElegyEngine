// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets
{
	public struct ApplicationConfig
	{
		public ApplicationConfig()
		{

		}

		public string Title { get; set; } = "unknown";
		public string Developer { get; set; } = "unknown";
		public string Publisher { get; set; } = "unknown";
		public string Version { get; set; } = "v0.00-dev";

		public string[] Plugins { get; set; } = Array.Empty<string>();
		public string[] Mounts { get; set; } = Array.Empty<string>();
	}
}
