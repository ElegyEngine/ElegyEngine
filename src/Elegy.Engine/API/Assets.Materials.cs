// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.API
{
	/// <summary>
	/// Asset system.
	/// </summary>
	public static partial class Assets
	{
		/// <summary>
		/// Loads a material from the material library.
		/// </summary>
		/// <param name="materialName">Name of the material.</param>
		/// <returns>
		/// A valid <see cref="Material"/> instance always, missing material if not found.
		/// </returns>
		public static Material LoadMaterial( string materialName )
			=> mAssetSystem.LoadMaterial( materialName );

		/// <summary>
		/// Unloads the <paramref name="material"/>. It is a reference, so it will
		/// become <see langword="null"/> after this.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> on success, <see langword="false"/>
		/// if the material is invalid or otherwise.
		/// </returns>
		public static bool UnloadMaterial( ref Material? material )
			=> mAssetSystem.UnloadMaterial( ref material );

		/// <summary>
		/// All the materials in the system.
		/// </summary>
		public static IEnumerable<Material> AllMaterials => mAssetSystem.GetMaterialList();
	}
}
