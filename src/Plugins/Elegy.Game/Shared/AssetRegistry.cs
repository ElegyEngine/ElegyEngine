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
		public string LevelName { get; init; } = string.Empty;
		public Dictionary<string, Model?> Models { get; } = new();
		public Dictionary<string, Material?> Materials { get; } = new();
		public Dictionary<string, string?> Sounds { get; } = new();
		public Dictionary<string, string?> OtherFiles { get; } = new();
	}
}
