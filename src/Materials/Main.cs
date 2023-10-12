// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Material system.
	/// </summary>
	public static partial class Materials
	{
		/// <summary>
		/// Loads a material from the material library.
		/// </summary>
		/// <param name="materialName">Name of the material.</param>
		/// <returns>
		/// A valid <see cref="Material"/> instance on success, <see langword="null"/> on failure.
		/// </returns>
		public static Material? LoadMaterial( string materialName )
			=> mMaterialSystem.LoadMaterial( materialName );

		/// <summary>
		/// Unloads the <paramref name="material"/>. It is a reference, so it will
		/// become <see langword="null"/> after this.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> on success, <see langword="false"/>
		/// if the material is invalid or otherwise.
		/// </returns>
		public static bool UnloadMaterial( ref Material material )
			=> mMaterialSystem.UnloadMaterial( ref material );

		/// <summary>
		/// All the materials in the system.
		/// </summary>
		public static IEnumerable<Material> AllMaterials => mMaterialSystem.GetMaterialList();
	}
}
