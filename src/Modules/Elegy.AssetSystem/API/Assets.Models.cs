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
		/// Registers a model loader plugin. If possible, you should prefer using Elegy.PluginSystem to
		/// add model loaders, as they support automatic unloading too.
		/// </summary>
		public static bool RegisterModelLoader( IModelLoader modelLoader )
		{
			if ( mModelLoaders.Contains( modelLoader ) )
			{
				return false;
			}

			mModelLoaders.Add( modelLoader );
			return true;
		}

		/// <summary>
		/// Unregisters a model loader plugin. If possible, you should prefer Elegy.PluginSystem to
		/// add/remove model loaders, as they support automatic unloading.
		/// </summary>
		public static bool UnregisterModelLoader( IModelLoader modelLoader )
		{
			if ( !mModelLoaders.Contains( modelLoader ) )
			{
				return false;
			}

			mModelLoaders.Remove( modelLoader );
			return true;
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

		/// <summary>
		/// A collection of all model loaders.
		/// </summary>
		public static IReadOnlyList<IModelLoader> ModelLoaders => mModelLoaders;
	}
}
