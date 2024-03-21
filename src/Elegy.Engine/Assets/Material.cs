// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces.Rendering;

namespace Elegy.Engine.Assets
{
	/// <summary>
	/// A material instance.
	/// </summary>
	public class Material
	{
		public string ResourceName { get; set; } = string.Empty;

		public IMaterial? RenderMaterial { get; set; } = null;
	}
}
