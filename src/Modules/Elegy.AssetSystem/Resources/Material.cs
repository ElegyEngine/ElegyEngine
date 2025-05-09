// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.AssetSystem.API;
using Elegy.AssetSystem.Interfaces.Rendering;

namespace Elegy.AssetSystem.Resources
{
	/// <summary>
	/// A material instance.
	/// </summary>
	public class Material
	{
		/// <summary></summary>
		public Material( MaterialDefinition data )
		{
			Data = data;
		}

		/// <summary></summary>
		public string Name => Data.Name;

		/// <summary></summary>
		public MaterialDefinition Data { get; }

		/// <summary></summary>
		public IMaterial? RenderMaterial { get; init; }
	}
}
