﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.AssetSystem.Resources;
using Elegy.Common.Assets;
using Elegy.ConsoleSystem;
using Elegy.FileSystem.API;

namespace Elegy.AssetSystem.API
{
	public static partial class Assets
	{
		internal class MaterialDefinitionPair
		{
			public MaterialDefinitionPair( MaterialDefinition def, Material? material )
			{
				Def = def;
				Material = material;
			}

			public MaterialDefinition Def { get; set; }
			public Material? Material { get; set; } = null;
		}

		private static TaggedLogger mLogger = new( "AssetSystem" );

		internal static List<IModelLoader> mModelLoaders = new();
		internal static List<ILevelLoader> mLevelLoaders = new();
		internal static List<ILevelWriter> mLevelWriters = new();
		internal static List<ITextureLoader> mTextureLoaders = new();

		private static List<MaterialDocument> mMaterialDocuments = new();
		private static Dictionary<string, MaterialDefinitionPair> mMaterialDefs = new();
		private static Dictionary<string, Texture> mTextures = new();
		private static Dictionary<string, ElegyMapDocument> mLevels = new();
		private static Dictionary<string, Model> mModels = new();

		internal static Texture mMissingTexture;

		private static Func<MaterialDefinition, IMaterial?>? mRenderMaterialFactory = null;
		private static Func<TextureMetadata, byte[], ITexture?>? mRenderTextureFactory = null;

		private static IEnumerable<Material> GetMaterialList()
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

		private static bool InitMaterials()
		{
			var loadMaterialsForDirectory = ( string name, string directory ) =>
			{
				string? path = Files.PathTo( $"{directory}/materials", PathFlags.Directory );
				if ( path is null )
				{
					mLogger.Error( $"{name} directory doesn't exist or doesn't have any materials!" );
					return false;
				}

				var materialDocumentPaths = Files.GetEntries( path, "*.shader", PathFlags.File, recursive: true );
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

					mMaterialDocuments.Add( document );

					foreach ( var materialDef in document.Materials )
					{
						// This way materials will be overridden
						mMaterialDefs[materialDef.Name] = new( materialDef, null );
					}

					mLogger.Developer( $"Parsed {document.Materials.Count} materials in '{materialDocumentPath}'" );
				}

				mLogger.Success( $"Parsed {mMaterialDefs.Count} materials!" );

				return true;
			};

			mLogger.Log( "Loading engine materials..." );
			if ( !loadMaterialsForDirectory( "Engine", Files.EnginePath ) )
			{
				return false;
			}

			foreach ( var mount in Files.CurrentConfig.Mounts )
			{
				loadMaterialsForDirectory( $"Mounted game {mount}", mount );
			}

			loadMaterialsForDirectory( $"This game", Files.CurrentGamePath );

			return true;
		}

		private static bool CreateMissingTexture()
		{
			mMissingTexture = CreateTexture( new()
				{
					Width = 32,
					Height = 32,
					Depth = 1,
					Components = 4,
					BytesPerPixel = 1,
					Srgb = false
				},
				Texture.GenerateMissingTexturePattern( 32, 32 ) );

			return true;
		}

		private static Texture CreateTexture( TextureMetadata metadata, byte[] bytes )
			=> new( metadata )
			{
				RenderTexture = mRenderTextureFactory?.Invoke( metadata, bytes ) ?? null
			};

		private static Material CreateMaterial( MaterialDefinition materialDefinition )
			=> new( materialDefinition )
			{
				RenderMaterial = mRenderMaterialFactory?.Invoke( materialDefinition ) ?? null
			};
	}
}
