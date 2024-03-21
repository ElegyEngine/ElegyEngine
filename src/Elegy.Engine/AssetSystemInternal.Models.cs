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
			return null;
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
