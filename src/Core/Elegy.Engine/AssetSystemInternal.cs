// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.API;
using Elegy.Engine.Interfaces;
using System.Linq;

namespace Elegy.Engine
{
	/// <summary>
	/// Asset system implementation.
	/// </summary>
	internal partial class AssetSystemInternal : IPluginCollector
	{
		private TaggedLogger mLogger = new( "AssetSystem" );

		private List<IModelLoader> mModelLoaders = new();
		private List<ITextureLoader> mTextureLoaders = new();

		public bool Init()
		{
			mLogger.Log( "Init" );

			Assets.SetAssetSystem( this );
			Plugins.RegisterPluginCollector( this );

			return InitMaterials();
		}

		private void AddLoader<TLoader>( List<TLoader> loaders, TLoader loader )
			where TLoader: IAssetLoader
		{
			if ( loaders.Contains( loader ) )
			{
				mLogger.Error( $"Tried loading an already registered plugin {loader.Name}" );
				return;
			}

			loaders.Add( loader );
		}

		private void RemoveLoader<TLoader>( List<TLoader> loaders, TLoader loader )
			where TLoader: IAssetLoader
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
				AddLoader( mModelLoaders, modelLoader );
			}
			else if ( plugin is ITextureLoader textureLoader )
			{
				AddLoader( mTextureLoaders, textureLoader );
			}
		}

		public void OnPluginUnloaded( IPlugin plugin )
		{
			if ( plugin is IModelLoader modelLoader )
			{
				RemoveLoader( mModelLoaders, modelLoader );
			}
			else if ( plugin is ITextureLoader textureLoader )
			{
				RemoveLoader( mTextureLoaders, textureLoader );
			}
		}

		public void Shutdown()
		{
			mTextures.Clear();
			mMaterialDefs.Clear();
		}
	}
}
