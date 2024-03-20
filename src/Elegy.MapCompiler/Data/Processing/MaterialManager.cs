// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// TODO: Replace with Elegy's material system when it has one

using Elegy.Common.Assets;

namespace Elegy.MapCompiler.Data.Processing
{
	/// <summary>
	/// Material system implementation.
	/// </summary>
	internal static class MaterialSystem
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

		private static Dictionary<string, MaterialDefinitionPair> mMaterialDefs = new();
		private static Material mDefaultMaterial = new()
		{
			Name = "Default",
			Width = 128,
			Height = 128
		};

		public static bool Init()
		{
			Console.WriteLine( $"[{Tag}] Init" );

			var loadMaterialsForDirectory = ( string name, string directory ) =>
			{
				string path = $"{directory}/materials";
				if ( !Path.Exists( path ) )
				{
					Console.WriteLine( $"WARNING: [{Tag}] {name} directory doesn't exist or doesn't have any materials!" );
					return false;
				}

				var materialDocumentPaths = Directory.GetFileSystemEntries( path, "*.shader", SearchOption.AllDirectories );
				if ( materialDocumentPaths.Length == 0 )
				{
					Console.WriteLine( $"WARNING: [{Tag}] {name}'s materials directory is empty!" );
					return false;
				}

				foreach ( var materialDocumentPath in materialDocumentPaths )
				{
					MaterialDocument document = new( File.ReadAllText( materialDocumentPath ) );
					if ( document.Materials.Count == 0 )
					{
						Console.WriteLine( $"WARNING: [{Tag}] Parsed 0 materials in '{materialDocumentPath}'" );
						continue;
					}

					foreach ( var materialDef in document.Materials )
					{
						// This way materials will be overridden
						mMaterialDefs[materialDef.Name] = new( materialDef, null );
					}

					Console.WriteLine( $"[{Tag}] Parsed {document.Materials.Count} materials in '{materialDocumentPath}'" );
				}

				return true;
			};

			Console.WriteLine( $"[{Tag}] Loading engine materials..." );
			if ( !loadMaterialsForDirectory( "Engine", "engine" ) )
			{
				return false;
			}

			loadMaterialsForDirectory( $"This game", FileSystem.GameDirectory );

			return true;
		}

		public static void Shutdown()
		{
			mMaterialDefs.Clear();
		}

		public static Material LoadMaterial( string materialName )
		{
			if ( mMaterialDefs.ContainsKey( materialName ) )
			{
				var pair = mMaterialDefs[materialName];
				if ( pair.Material is null )
				{
					pair.Material = CreateMaterialFromDef( pair.Def );
				}

				return pair.Material;
			}

			Console.WriteLine( $"WARNING: [{Tag}] Material '{materialName}' doesn't exist" );

			return mDefaultMaterial;
		}

		private static Material CreateMaterialFromDef( MaterialDefinition materialDef )
		{
			Material material = ImageLoader.LoadMaterialFromPng( materialDef.DiffuseMap ?? "none" ) ?? new()
			{
				Width = 128,
				Height = 128,
			};

			// You can populate other tool material parameters here
			material.Name = materialDef.Name;
			material.Flags = materialDef.ToolFlags;

			return material;
		}
	}
}
