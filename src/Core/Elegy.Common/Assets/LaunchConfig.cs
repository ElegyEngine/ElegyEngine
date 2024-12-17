// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;

namespace Elegy.Common.Assets
{
	/// <summary></summary>
	public class LaunchConfig
	{
		/// <summary>Path to engine configuration. Overrides <see cref="Engine"/>.</summary>
		public string? EngineConfigName { get; set; }

		/// <summary>Engine configuration. By default will load 'engineConfig.json'.</summary>
		public EngineConfig Engine { get; set; } = new();

		/// <summary>Commandline arguments.</summary>
		public string[] Args { get; set; } = [];

		/// <summary>Console frontends to be loaded up front.</summary>
		public IPlugin[]? ConsoleFrontends { get; set; }

		/// <summary>Whether to create a main window or not at the start.</summary>
		public bool WithMainWindow { get; set; } = true;

		/// <summary>If set to <c>true</c>, will not load application plugins.</summary>
		public bool ToolMode { get; set; }
	}
}
