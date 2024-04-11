// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Resources;
using Elegy.Common.Assets;
using Elegy.FileSystem.API;

namespace Elegy.AssetSystem.API
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
		{
			if ( mMaterialDefs.ContainsKey( materialName ) )
			{
				var pair = mMaterialDefs[materialName];
				if ( pair.Material is null )
				{
					pair.Material = CreateMaterial( pair.Def );
				}

				return pair.Material;
			}

			mLogger.Warning( $"Material '{materialName}' doesn't exist" );
			return null;
		}

		/// <summary>
		/// Unloads the <paramref name="material"/>. It is a reference, so it will
		/// become <see langword="null"/> after this.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> on success, <see langword="false"/>
		/// if the material is invalid or otherwise.
		/// </returns>
		public static bool UnloadMaterial( ref Material? material )
		{
			if ( material == null )
			{
				return false;
			}

			if ( !mMaterialDefs.ContainsKey( material.Name ) )
			{
				return false;
			}

			mMaterialDefs[material.Name].Material = null;
			material = null;

			return true;
		}

		/// <summary>
		/// Loads and returns a <see cref="Texture"/>.
		/// </summary>
		public static Texture? LoadTexture( string texturePath )
		{
			string? fullPath = Files.PathTo( texturePath, PathFlags.File );
			if ( fullPath is null )
			{
				mLogger.Error( $"LoadTexture: Can't find texture '{texturePath}'" );
				return MissingTexture;
			}

			string extension = Path.GetExtension( texturePath ) ?? "";
			ITextureLoader? textureLoader = FindTextureLoader( [extension] );
			if ( textureLoader is null )
			{
				mLogger.Error( $"LoadTexture: Unsupported format '{extension}'" );
				return MissingTexture;
			}

			(TextureMetadata? textureMetadata, byte[]? data) = textureLoader.LoadTexture( fullPath, false );
			if ( data is null )
			{
				mLogger.Error( $"LoadTexture: Couldn't load data for texture '{texturePath}'" );
				return MissingTexture;
			}

			mTextures[texturePath] = CreateTexture( textureMetadata, data );
			return mTextures[texturePath];
		}

		/// <summary>
		/// Finds an appropriate <see cref="ITextureLoader"/> according to one of the <paramref name="extensions"/>.
		/// </summary>
		public static ITextureLoader? FindTextureLoader( params string[] extensions )
		{
			foreach ( var textureLoader in mTextureLoaders )
			{
				foreach ( var extension in extensions )
				{
					if ( textureLoader.CanLoad( extension ) )
					{
						return textureLoader;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// All the materials in the system.
		/// </summary>
		public static IEnumerable<Material> AllMaterials => GetMaterialList();

		/// <summary>
		/// The "missing texture" texture.
		/// </summary>
		public static Texture MissingTexture => mMissingTexture;
	}
}
