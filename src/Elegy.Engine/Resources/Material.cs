// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.Interfaces.Rendering;

namespace Elegy.Engine.Resources
{
	/// <summary>
	/// A material instance.
	/// </summary>
	public class Material
	{
		public string ResourceName { get; set; } = string.Empty;

		public MaterialDefinition Data { get; set; }

		public IMaterial? RenderMaterial { get; set; } = null;
	}
}
