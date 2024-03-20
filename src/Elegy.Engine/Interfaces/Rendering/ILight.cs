// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Elegy.Engine.Interfaces.Rendering
{
	/// <summary>
	/// 
	/// </summary>
	public enum LightType
	{
		/// <summary>
		/// Omnidirectional light, like a light bulb.
		/// </summary>
		Omni,

		/// <summary>
		/// Directional spotlight, like a lamp or a flashlight.
		/// <see cref="ILight.Data"/> mapping:
		/// X = inner angle,
		/// Y = outer angle
		/// </summary>
		Spotlight,

		/// <summary>
		/// Directional sun/moonlight.
		/// </summary>
		Directional,

		/// <summary>
		/// Special kind of light which controls lightmaps.
		/// <see cref="ILight.Data"/> mapping:
		/// X = lightmap ID
		/// </summary>
		LightmapController
	}

	/// <summary>
	/// RenderFrame light.
	/// </summary>
	public interface ILight
	{
		/// <summary>
		/// Layers that the light will be rendered to.
		/// </summary>
		int Mask { get; set; }

		/// <summary>
		/// Red, green, blue, alpha.
		/// </summary>
		Vector4 Colour { get; set; }

		/// <summary>
		/// Position of the light.
		/// </summary>
		Vector3 Position { get; set; }

		/// <summary>
		/// The type of light.
		/// </summary>
		LightType Type { get; set; }

		/// <summary>
		/// Associated light data, depending on the <see cref="Type"/>.
		/// </summary>
		Vector4 Data { get; set; }
	}
}
