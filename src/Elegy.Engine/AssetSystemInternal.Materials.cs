// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.API;
using Elegy.Engine.Interfaces;
using Elegy.Engine.Resources;
using System.IO;

namespace Elegy.Engine
{
	/// <summary>
	/// Material system implementation.
	/// </summary>
	internal partial class AssetSystemInternal
	{
		class MaterialDefinitionPair
		{
			public MaterialDefinitionPair( MaterialDefinition def, Material? material )
			{
				Def = def;
				Material = material;
			}

			public MaterialDefinition Def { get; set; }
			public Material? Material { get; set; } = null;
		}

		private Dictionary<string, MaterialDefinitionPair> mMaterialDefs = new();
		private Dictionary<string, Texture> mTextures = new();

		public Texture MissingTexture { get; private set; }

		private bool InitMaterials()
		{
			var loadMaterialsForDirectory = ( string name, string directory ) =>
			{
				string? path = FileSystem.PathTo( $"{directory}/materials", PathFlags.Directory );
				if ( path is null )
				{
					mLogger.Error( $"{name} directory doesn't exist or doesn't have any materials!" );
					return false;
				}

				var materialDocumentPaths = FileSystem.GetEntries( path, "*.shader", PathFlags.File, true );
				if ( materialDocumentPaths is null || materialDocumentPaths.Length == 0 )
				{
					mLogger.Error( $"{name}'s materials directory is empty!" );
					return false;
				}

				foreach ( var materialDocumentPath in materialDocumentPaths )
				{
					MaterialDocument document = new( File.ReadAllText( materialDocumentPath ) );
					if ( document.Materials.Count == 0 )
					{
						mLogger.Warning( $"Parsed 0 materials in '{materialDocumentPath}'" );
						continue;
					}

					foreach ( var materialDef in document.Materials )
					{
						// This way materials will be overridden
						mMaterialDefs[materialDef.Name] = new( materialDef, null );
					}

					mLogger.Success( $"Parsed {document.Materials.Count} materials in '{materialDocumentPath}'" );
				}

				return true;
			};

			mLogger.Log( "Loading engine materials..." );
			if ( !loadMaterialsForDirectory( "Engine", FileSystem.EnginePath ) )
			{
				return false;
			}

			foreach ( var mount in FileSystem.CurrentConfig.Mounts )
			{
				loadMaterialsForDirectory( $"Mounted game {mount}", mount );
			}

			loadMaterialsForDirectory( $"This game", FileSystem.CurrentGamePath );

			return CreateMissingTexture();
		}

		private bool CreateMissingTexture()
		{
			MissingTexture = Texture.FromData( new TextureMetadata()
			{
				Width = 16,
				Height = 16,
				Depth = 0,
				Components = 4,
				BytesPerPixel = 1
			},
			Texture.GenerateMissingTexturePattern() );
			return true;
		}

		public Material? LoadMaterial( string materialName )
		{
			if ( mMaterialDefs.ContainsKey( materialName ) )
			{
				var pair = mMaterialDefs[materialName];
				if ( pair.Material is null )
				{
					pair.Material = new( pair.Def );
				}

				return pair.Material;
			}

			mLogger.Warning( $"Material '{materialName}' doesn't exist" );
			return null;
		}

		public Texture LoadTexture( string texturePath )
		{
			string? fullPath = FileSystem.PathTo( texturePath, PathFlags.File );
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

			mTextures[texturePath] = Texture.FromData( textureMetadata, data );
			return mTextures[texturePath];
		}

		public ITextureLoader? FindTextureLoader( string[] extensions )
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

		public bool UnloadMaterial( ref Material? material )
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

		public IEnumerable<Material> GetMaterialList()
		{
			foreach ( var pair in mMaterialDefs )
			{
				if ( pair.Value.Material is null )
				{
					continue;
				}

				yield return pair.Value.Material;
			}
		}
	}
}
