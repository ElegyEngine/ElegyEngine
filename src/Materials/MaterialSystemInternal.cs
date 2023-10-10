// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace Elegy
{
	/// <summary>
	/// Material system implementation.
	/// </summary>
	internal sealed class MaterialSystemInternal
	{
		private List<Material> mMaterials = new();
		private Dictionary<string, MaterialDefinition> mMaterialDefs = new();

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
						mMaterialDefs[materialDef.Name] = materialDef;
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
			mMaterials.Clear();
			mMaterialDefs.Clear();
		}

		public Material? LoadMaterial( string materialName )
		{
			

			return null;
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

		public IReadOnlyList<Material> GetMaterialList() => mMaterials;
	}
}
