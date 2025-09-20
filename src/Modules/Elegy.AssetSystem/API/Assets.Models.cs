// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.AssetSystem.Interfaces;

namespace Elegy.AssetSystem.API
{
	public static partial class Assets
	{
		/// <summary>
		/// Loads a model from the path.
		/// </summary>
		public static Model? LoadModel( string path )
		{
			string? fullPath = mFileSystem.PathToFile( path );
			if ( fullPath is null )
			{
				mLogger.Error( $"LoadModel: Can't find model '{path}'" );
				return null;
			}

			string extension = Path.GetExtension( path );
			IModelLoader? modelLoader = FindModelLoader( extension );
			if ( modelLoader is null )
			{
				mLogger.Error( $"LoadModel: Unsupported format '{extension}'" );
				return null;
			}

			Model? model = modelLoader.LoadModel( fullPath );
			if ( model is null )
			{
				mLogger.Error( $"LoadModel: Cannot load model '{path}'\nFull path: {fullPath}" );
				return null;
			}

			model.Name = path;
			mModels[path] = model;
			return model;
		}

		/// <summary>
		/// Finds an appropriate <see cref="IModelLoader"/> according to one of the <paramref name="extensions"/>.
		/// </summary>
		public static IModelLoader? FindModelLoader( params string[] extensions )
		{
			foreach ( var modelLoader in mModelLoaders )
			{
				for ( int i = 0; i < extensions.Length; i++ )
				{
					if ( modelLoader.Supports( extensions[i] ) )
					{
						return modelLoader;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// A collection of all loaded models.
		/// </summary>
		public static IReadOnlyCollection<Model> AllModels => mModels.Values;
	}
}
