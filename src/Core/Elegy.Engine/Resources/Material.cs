// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.API;
using Elegy.Engine.Interfaces.Rendering;

namespace Elegy.Engine.Resources
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
			RenderMaterial = Render.Instance.CreateMaterial( this );
		}

		/// <summary></summary>
		public string Name => Data.Name;

		/// <summary></summary>
		public MaterialDefinition Data { get; }

		/// <summary></summary>
		public IMaterial? RenderMaterial { get; } = null;
	}
}
