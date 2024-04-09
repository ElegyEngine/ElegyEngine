// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;

namespace Elegy.AssetSystem
{
	internal class AssetPluginCollector : IPluginCollector
	{
		private TaggedLogger mLogger = new( "AssetSystem" );

		private void AddLoader<TLoader>( List<TLoader> loaders, TLoader loader )
			where TLoader : IAssetLoader
		{
			if ( loaders.Contains( loader ) )
			{
				mLogger.Error( $"Tried loading an already registered plugin {loader.Name}" );
				return;
			}

			loaders.Add( loader );
		}

		private void RemoveLoader<TLoader>( List<TLoader> loaders, TLoader loader )
			where TLoader : IAssetLoader
		{
			if ( !loaders.Remove( loader ) )
			{
				mLogger.Error( $"Tried unloading non-registered plugin {loader.Name}" );
			}
		}

		public void OnPluginLoaded( IPlugin plugin )
		{
			if ( plugin is IModelLoader modelLoader )
			{
				AddLoader( API.Assets.mModelLoaders, modelLoader );
			}
			else if ( plugin is ITextureLoader textureLoader )
			{
				AddLoader( API.Assets.mTextureLoaders, textureLoader );
			}
		}

		public void OnPluginUnloaded( IPlugin plugin )
		{
			if ( plugin is IModelLoader modelLoader )
			{
				RemoveLoader( API.Assets.mModelLoaders, modelLoader );
			}
			else if ( plugin is ITextureLoader textureLoader )
			{
				RemoveLoader( API.Assets.mTextureLoaders, textureLoader );
			}
		}
	}
}
