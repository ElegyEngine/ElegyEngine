// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets
{
	/// <summary></summary>
	public struct ApplicationConfig
	{
		/// <summary></summary>
		public ApplicationConfig()
		{

		}

		/// <summary></summary>
		public string Title { get; set; } = "unknown";
		/// <summary></summary>
		public string Developer { get; set; } = "unknown";
		/// <summary></summary>
		public string Publisher { get; set; } = "unknown";
		/// <summary></summary>
		public string Version { get; set; } = "v0.00-dev";

		/// <summary></summary>
		public string[] Plugins { get; set; } = Array.Empty<string>();
		/// <summary></summary>
		public string[] Mounts { get; set; } = Array.Empty<string>();

		/// <summary></summary>
		public string RenderFrontend { get; set; } = "engine/plugins/RenderStandard";
		/// <summary></summary>
		public string AudioSystem { get; set; } = "engine/plugins/AudioLoudPizza";
	}
}
