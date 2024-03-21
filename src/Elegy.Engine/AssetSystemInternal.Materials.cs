// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.API;

namespace Elegy.Engine
{
	/// <summary>
	/// IMaterial system implementation.
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
		private Dictionary<string, Texture2D> mTextures = new();

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

			return true;
		}

		public Material LoadMaterial( string materialName )
		{
			if ( mMaterialDefs.ContainsKey( materialName ) )
			{
				var pair = mMaterialDefs[materialName];
				if ( pair.Material is null )
				{
					pair.Material = LoadMaterial( pair.Def );
				}

				return pair.Material;
			}

			mLogger.Warning( $"Material '{materialName}' doesn't exist" );

			return new()
			{
				ResourceName = materialName
			};
		}

		private Material LoadMaterial( MaterialDefinition materialDef )
		{
			Texture2D? texture = LoadTexture( materialDef.DiffuseMap );
			if ( texture is null )
			{
				return new()
				{
					ResourceName = "Default"
				};
			}

			return new()
			{
				ResourceName = materialDef.Name
			};
		}

		// Texture extensions have some type of priority here
		// First it checks if there's a KTX (which is the best all-rounder IMO), then TGA, DDS etc.
		// Later we'll replace these with texture loading plugins (for e.g. sprite support)
		private static readonly string[] TextureExtensions =
		{
			".ktx", ".tga", ".dds", ".png", ".jpg", ".jpeg", ".webp", ".bmp"
		};

		private Texture2D? LoadTexture( string? texturePath )
		{
			if ( texturePath is null )
			{
				return null;
			}

			if ( Path.HasExtension( texturePath ) )
			{
				return Texture2D.CreateFromImageFile( texturePath );
			}

			foreach ( var textureExtension in TextureExtensions )
			{
				string? fullTexturePath = FileSystem.PathTo( $"{texturePath}{textureExtension}" );
				if ( fullTexturePath is null )
				{
					continue;
				}

				return Texture2D.CreateFromImageFile( fullTexturePath );
			}

			return null;
		}

		public bool UnloadMaterial( ref Material? material )
		{
			if ( material == null )
			{
				return false;
			}

			if ( !mMaterialDefs.ContainsKey( material.ResourceName ) )
			{
				return false;
			}

			mMaterialDefs[material.ResourceName].Material = null;
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
