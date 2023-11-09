// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using System.IO;

namespace Elegy
{
	/// <summary>
	/// Material system implementation.
	/// </summary>
	internal sealed class MaterialSystemInternal
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

		private const string Tag = "MaterialManager";

		private Dictionary<string, MaterialDefinitionPair> mMaterialDefs = new();
		private Dictionary<string, Texture2D> mTextures = new();

		public bool Init()
		{
			Console.Log( Tag, "Init" );

			Materials.SetMaterialSystem( this );

			var loadMaterialsForDirectory = ( string name, string directory ) =>
			{
				string? path = FileSystem.PathTo( $"{directory}/materials", PathFlags.Directory );
				if ( path is null )
				{
					Console.Error( Tag, $"{name} directory doesn't exist or doesn't have any materials!" );
					return false;
				}

				var materialDocumentPaths = FileSystem.GetEntries( path, "*.shader", PathFlags.File, true );
				if ( materialDocumentPaths is null || materialDocumentPaths.Length == 0 )
				{
					Console.Error( Tag, $"{name}'s materials directory is empty!" );
					return false;
				}

				foreach ( var materialDocumentPath in materialDocumentPaths )
				{
					MaterialDocument document = new( File.ReadAllText( materialDocumentPath ) );
					if ( document.Materials.Count == 0 )
					{
						Console.Warning( Tag, $"Parsed 0 materials in '{materialDocumentPath}'" );
						continue;
					}

					foreach ( var materialDef in document.Materials )
					{
						// This way materials will be overridden
						mMaterialDefs[materialDef.Name] = new( materialDef, null );
					}

					Console.Success( Tag, $"Parsed {document.Materials.Count} materials in '{materialDocumentPath}'" );
				}

				return true;
			};

			Console.Log( Tag, "Loading engine materials..." );
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

		public void Shutdown()
		{
			mTextures.Clear();
			mMaterialDefs.Clear();
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

			Console.Warning( Tag, $"Material '{materialName}' doesn't exist" );

			return new StandardMaterial3D()
			{
				AlbedoColor = Color.Color8( 255, 128, 192 ),
				Roughness = 0.5f,
				Metallic = 0.5f
			};
		}

		private Material LoadMaterial( MaterialDefinition materialDef )
		{
			Texture2D? texture = LoadTexture( materialDef.DiffuseMap );
			if ( texture is null )
			{
				return new StandardMaterial3D()
				{
					AlbedoColor = Color.Color8( 255, 128, 192 ),
					Roughness = 0.5f,
					Metallic = 0.5f
				};
			}

			return new StandardMaterial3D()
			{
				ResourceName = materialDef.Name,

				Roughness = 1.0f,
				Metallic = 0.0f,
				MetallicSpecular = 0.0f,
				SpecularMode = BaseMaterial3D.SpecularModeEnum.Disabled,
				TextureFilter = BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic,

				AlbedoTexture = texture
			};
		}

		private Texture2D? LoadTexture( string? texturePath )
		{
			if ( texturePath is null )
			{
				return null;
			}

			string? fullTexturePath = FileSystem.PathTo( texturePath );
			if ( fullTexturePath is null )
			{
				return null;
			}

			return ImageTexture.CreateFromImage( Image.LoadFromFile( fullTexturePath ) );
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
