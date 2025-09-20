// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Resources;
using Elegy.Common.Assets;

// TODO: Material loader interface. Right now, materials are essentially
// Quake 3 style documents describing multiple materials in one file. This
// should be extendable to support different kinds of material libraries,
// like WADs, plain texture images and so on

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

		private static (TextureMetadata? textureMetadata, byte[]? data) LoadTextureInternal( string texturePath,
			bool withoutData, bool hintSrgb )
		{
			string? fullPath = mFileSystem.PathToFile( texturePath );
			if ( fullPath is null )
			{
				mLogger.Error( $"LoadTexture: Can't find texture '{texturePath}'" );
				return (null, null);
			}

			string extension = Path.GetExtension( texturePath ) ?? "";
			ITextureLoader? textureLoader = FindTextureLoader( [extension] );
			if ( textureLoader is null )
			{
				mLogger.Error( $"LoadTexture: Unsupported format '{extension}'" );
				return (null, null);
			}

			(TextureMetadata? textureMetadata, byte[]? data)
				= textureLoader.LoadTexture( fullPath, withoutData, hintSrgb );
			if ( data is null && !withoutData )
			{
				mLogger.Error( $"LoadTexture: Couldn't load data for texture '{texturePath}'" );
				return (null, null);
			}

			return (textureMetadata, data);
		}

		/// <summary>
		/// Loads and returns a <see cref="Texture"/>.
		/// Caches the result so it can be easily retrieved if the same texture
		/// is referenced by multiple materials.
		/// </summary>
		public static Texture? LoadTexture( string texturePath, bool hintSrgb = false )
		{
			if ( mTextures.ContainsKey( texturePath ) )
			{
				return mTextures[texturePath];
			}

			(TextureMetadata? textureMetadata, byte[]? data)
				= LoadTextureInternal( texturePath, withoutData: false, hintSrgb );
			if ( textureMetadata is null || data is null )
			{
				return MissingTexture;
			}

			mTextures[texturePath] = CreateTexture( textureMetadata, data );
			return mTextures[texturePath];
		}

		/// <summary>
		/// Loads and returns a <see cref="TextureMetadata"/>.
		/// This result is not cached, unlike <see cref="LoadTexture"/>.
		/// </summary>
		public static TextureMetadata? GetTextureMetadata( string texturePath )
			=> LoadTextureInternal( texturePath, withoutData: true, hintSrgb: false ).textureMetadata;

		/// <summary>
		/// Registers a texture loader plugin. If possible, you should prefer using Elegy.PluginSystem to
		/// add texture loaders, as they support automatic unloading too.
		/// </summary>
		public static bool RegisterTextureLoader( ITextureLoader textureLoader )
		{
			if ( mTextureLoaders.Contains( textureLoader ) )
			{
				return false;
			}

			mTextureLoaders.Add( textureLoader );
			return true;
		}

		/// <summary>
		/// Unregisters a texture loader plugin. If possible, you should prefer Elegy.PluginSystem to
		/// add/remove texture loaders, as they support automatic unloading.
		/// </summary>
		public static bool UnregisterTextureLoader( ITextureLoader textureLoader )
		{
			if ( !mTextureLoaders.Contains( textureLoader ) )
			{
				return false;
			}

			mTextureLoaders.Remove( textureLoader );
			return true;
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
					if ( textureLoader.Supports( extension ) )
					{
						return textureLoader;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Adds a brand-new material document, describing one or more materials.
		/// </summary>
		public static void AddMaterialDocument( MaterialDocument materialDocument )
		{
			mMaterialDocuments.Add( materialDocument );
			foreach ( var materialDef in materialDocument.Materials )
			{
				// This way materials will be overridden
				mMaterialDefs[materialDef.Name] = new( materialDef, null );
			}
		}

		/// <summary>
		/// All the materials in the system.
		/// </summary>
		public static IEnumerable<Material> AllMaterials => GetMaterialList();

		/// <summary>
		/// A collection of all texture loaders.
		/// </summary>
		public static IReadOnlyList<ITextureLoader> TextureLoaders => mTextureLoaders;

		/// <summary>
		/// The "missing texture" texture.
		/// </summary>
		public static Texture MissingTexture => mMissingTexture;
	}
}
