// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// TODO: Make this a record struct

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
			//"plugins/RenderStyle99",
			// We don't have audio yet, so this one shall wait
			//"plugins/AudioStandard",
			"plugins/DevConsole"
		];

		/// <summary>
		/// Game with moddable game directory support.
		/// </summary>
		public static EngineConfig Game( string gameFolder, params string[] enginePlugins )
			=> new()
			{
				BaseFolder = gameFolder,
				EnginePlugins = enginePlugins
			};

		/// <summary>
		/// Suitable for applications that don't need moddable game directories.
		/// </summary>
		public static EngineConfig App( params string[] enginePlugins )
			=> new()
			{
				BaseFolder = "./",
				EnginePlugins = enginePlugins
			};
	}
}
