// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.RenderSystem.Objects
{
	/// <summary>
	/// A light type. Not all are supported by a render style.
	/// Some render styles might only implement the first 3 or 4, others
	/// might exclusively implement the <see cref="Lightmap"/> type.
	/// </summary>
	public enum LightType
	{
		Omni,
		Spot,
		Directional,
		Surface,
		Lightmap,
		Grid,

		Custom1,
		Custom2,
		Custom3,
		Custom4
	}

	public class Light
	{
	}
}
