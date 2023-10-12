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
		private Dictionary<string, Tuple<MaterialDefinition, Material?>> mMaterialDefs = new();
		private Dictionary<string, Texture2D> mTextures = new();

		public bool Init()
		{
			Console.Log( "MaterialSystem", "Init" );

			var loadMaterialsForDirectory = ( string name, string directory ) =>
			{
				string? path = FileSystem.PathTo( $"{directory}/materials", PathFlags.Directory );
				if ( path is null )
				{
					Console.Error( "MaterialSystem", $"{name} directory doesn't exist or doesn't have any materials!" );
					return false;
				}

				var materialDocumentPaths = FileSystem.GetEntries( path, "*.shader", PathFlags.File, true );
				if ( materialDocumentPaths is null || materialDocumentPaths.Length == 0 )
				{
					Console.Error( "MaterialSystem", $"{name}'s materials directory is empty!" );
					return false;
				}

				foreach ( var materialDocumentPath in materialDocumentPaths )
				{
					MaterialDocument document = new( File.ReadAllText( materialDocumentPath ) );
					if ( document.Materials.Count == 0 )
					{
						Console.Warning( "MaterialSystem", $"Parsed 0 materials in '{materialDocumentPath}'" );
						continue;
					}

					foreach ( var materialDef in document.Materials )
					{
						// This way materials will be overridden
						mMaterialDefs[materialDef.Name] = new( materialDef, null );
					}

					Console.Success( "MaterialSystem", $"Parsed {document.Materials.Count} materials in '{materialDocumentPath}'" );
				}

				return true;
			};

			Console.Log( "MaterialSystem", "Loading engine materials..." );
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

		public Material? LoadMaterial( string materialName )
		{
			if ( mMaterialDefs.ContainsKey( materialName ) )
			{
				var pair = mMaterialDefs[materialName];
				Material? material = pair.Item2;
				if ( material is null )
				{
					material = LoadMaterial( pair.Item1 );
				}

				return material;
			}

			Console.Warning( "MaterialSystem", $"Material '{materialName}' doesn't exist" );
			return null;
		}

		private Material LoadMaterial( MaterialDefinition materialDef )
		{
			return new StandardMaterial3D()
			{
				ResourceName = materialDef.Name,

				Roughness = 1.0f,
				Metallic = 0.0f,
				MetallicSpecular = 0.0f,
				SpecularMode = BaseMaterial3D.SpecularModeEnum.Disabled,
				TextureFilter = BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic,

				AlbedoTexture = LoadTexture( materialDef.DiffuseMap )
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

		public bool UnloadMaterial( ref Material material )
		{
			if ( material == null )
			{
				return false;
			}

			if ( mMaterials.Contains( material ) )
			{
				return false;
			}

			mMaterials.Remove( material );
			return true;
		}

		public IEnumerable<Material> GetMaterialList()
		{
			foreach ( var pair in mMaterialDefs )
			{
				if ( pair.Value.Item2 is null )
				{
					continue;
				}

				yield return pair.Value.Item2;
			}
		}
	}
}
