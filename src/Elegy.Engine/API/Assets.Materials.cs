// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces;
using Elegy.Engine.Resources;

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
		public static Material? LoadMaterial( string materialName )
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
		/// Loads and returns a <see cref="Texture"/>.
		/// </summary>
		public static Texture? LoadTexture( string texturePath )
			=> mAssetSystem.LoadTexture( texturePath );

		/// <summary>
		/// Finds an appropriate <see cref="ITextureLoader"/> according to one of the <paramref name="extensions"/>.
		/// </summary>
		public static ITextureLoader? FindTextureLoader( params string[] extensions )
			=> mAssetSystem.FindTextureLoader( extensions );

		/// <summary>
		/// All the materials in the system.
		/// </summary>
		public static IEnumerable<Material> AllMaterials => mAssetSystem.GetMaterialList();

		/// <summary>
		/// The "missing texture" texture.
		/// </summary>
		public static Texture MissingTexture => mAssetSystem.MissingTexture;
	}
}
