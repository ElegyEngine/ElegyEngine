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

		private void TryRegisterPlugin<TLoader>( List<TLoader> loaders, IPlugin loader )
			where TLoader : IAssetIo
		{
			if ( loader is TLoader typedLoader )
			{
				if ( loaders.Contains( typedLoader ) )
				{
					mLogger.Error( $"Tried loading an already registered plugin {loader.Name}" );
					return;
				}

				loaders.Add( typedLoader );
			}
		}

		private void TryUnregisterPlugin<TLoader>( List<TLoader> loaders, IPlugin loader )
			where TLoader : IAssetIo
		{
			if ( loader is TLoader typedLoader )
			{
				if ( !loaders.Remove( typedLoader ) )
				{
					mLogger.Error( $"Tried unloading non-registered plugin {loader.Name}" );
				}
			}
		}

		public void OnPluginLoaded( IPlugin plugin )
		{
			TryRegisterPlugin( API.Assets.mModelLoaders, plugin );
			TryRegisterPlugin( API.Assets.mTextureLoaders, plugin );
			TryRegisterPlugin( API.Assets.mLevelLoaders, plugin );
			TryRegisterPlugin( API.Assets.mLevelWriters, plugin );
		}

		public void OnPluginUnloaded( IPlugin plugin )
		{
			TryUnregisterPlugin( API.Assets.mModelLoaders, plugin );
			TryUnregisterPlugin( API.Assets.mTextureLoaders, plugin );
			TryUnregisterPlugin( API.Assets.mLevelLoaders, plugin );
			TryUnregisterPlugin( API.Assets.mLevelWriters, plugin );
		}
	}
}
