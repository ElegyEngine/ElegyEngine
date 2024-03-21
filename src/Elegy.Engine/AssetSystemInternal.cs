// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.API;
using Elegy.Engine.Interfaces;

namespace Elegy.Engine
{
	/// <summary>
	/// Asset system implementation.
	/// </summary>
	internal partial class AssetSystemInternal : IPluginCollector
	{
		private TaggedLogger mLogger = new( "AssetSystem" );

		private List<IModelLoader> mModelLoaders = new();

		public bool Init()
		{
			mLogger.Log( "Init" );

			Assets.SetAssetSystem( this );
			Plugins.RegisterPluginCollector( this );

			return InitMaterials();
		}

		public void OnPluginLoaded( IPlugin plugin )
		{
			if ( plugin is IModelLoader modelLoaderPlugin )
			{
				if ( mModelLoaders.Contains( modelLoaderPlugin ) )
				{
					mLogger.Error( $"Tried loading an already registered plugin {plugin.Name}" );
					return;
				}

				mModelLoaders.Add( modelLoaderPlugin );
			}
		}

		public void OnPluginUnloaded( IPlugin plugin )
		{
			if ( plugin is IModelLoader modelLoaderPlugin )
			{
				if ( !mModelLoaders.Remove( modelLoaderPlugin ) )
				{
					mLogger.Error( $"Tried unloading non-registered plugin {plugin.Name}" );
				}
			}
		}

		public void Shutdown()
		{
			mTextures.Clear();
			mMaterialDefs.Clear();
		}
	}
}
