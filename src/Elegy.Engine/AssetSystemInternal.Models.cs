// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.API;
using Elegy.Engine.Interfaces;

namespace Elegy.Engine
{
	internal partial class AssetSystemInternal
	{
		private Dictionary<string, Model> mModels;

		public Model? LoadModel( string path )
		{
			string? fullPath = FileSystem.PathTo( path, PathFlags.File );
			if ( fullPath is null )
			{
				mLogger.Error( $"LoadModel: Can't find model '{path}'" );
				return null;
			}

			string extension = Path.GetExtension( path ) ?? "";
			IModelLoader? modelLoader = FindModelLoader( [extension] );
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

		public IModelLoader? FindModelLoader( string[] extensions )
		{
			foreach ( var modelLoader in mModelLoaders )
			{
				for ( int i = 0; i < extensions.Length; i++ )
				{
					if ( modelLoader.CanLoad( extensions[i] ) )
					{
						return modelLoader;
					}
				}
			}

			return null;
		}

		public IReadOnlyCollection<Model> GetModelList()
			=> mModels.Values;
	}
}
