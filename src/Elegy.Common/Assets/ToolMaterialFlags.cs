// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Assets
{
	/// <summary>
	/// Compiling flags that can be had by materials.
	/// </summary>
	[Flags]
	public enum ToolMaterialFlag
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,

		/// <summary>
		/// This surface isn't drawn.
		/// </summary>
		NoDraw = 1,

		/// <summary>
		/// This is used to determine centres of brush entities.
		/// </summary>
		Origin = 2,

		/// <summary>
		/// This surface acts as a runtime occluder.
		/// </summary>
		Occluder = 4,

		/// <summary>
		/// Do not cast shadows e.g. when baking lightmaps.
		/// </summary>
		NoShadowCast = 8,

		/// <summary>
		/// Lightmap UV space won't be allocated for this material.
		/// </summary>
		NoLightmapReceived = 16,

		/// <summary>
		/// This surface has no collision.
		/// </summary>
		NoCollision = 32
	};
}
