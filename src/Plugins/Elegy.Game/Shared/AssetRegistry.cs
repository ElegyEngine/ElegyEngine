// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Resources;
using Elegy.Common.Assets;

namespace Game.Shared
{
	/// <summary>
	/// Manages resource strings. The server typically populates this list, and clients
	/// receive it, from which they precache assets.
	/// </summary>
	public class AssetRegistry
	{
		public Dictionary<string, Model?> Models { get; private set; } = new();
		public Dictionary<string, Material?> Materials { get; private set; } = new();
		public Dictionary<string, string?> Sounds { get; private set; } = new();
		public Dictionary<string, string?> OtherFiles { get; private set; } = new();
		public string LevelName { get; init; } = string.Empty;
	}
}
