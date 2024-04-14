// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.AssetSystem.Loaders;
using Elegy.Common.Assets;
using Elegy.PluginSystem.API;

namespace Elegy.AssetSystem.API
{
	public static partial class Assets
	{
		public static bool Init( in LaunchConfig config )
		{
			mLogger.Log( "Init" );

			Plugins.RegisterDependency( "Elegy.AssetSystem", typeof( Assets ).Assembly );
			Plugins.RegisterPluginCollector( new AssetPluginCollector() );

			Plugins.RegisterPlugin( new GltfModelLoader() );
			Plugins.RegisterPlugin( new ObjModelLoader() ); // it's not implemented but oh well
			Plugins.RegisterPlugin( new PngImageLoader() );

			return InitMaterials();
		}

		public static bool PostInit()
		{
			// The missing texture can be created once the renderer is initialised, so yeah
			return CreateMissingTexture();
		}

		public static void SetRenderFactories( Func<MaterialDefinition, IMaterial?> materialFactory, Func<TextureMetadata, byte[], ITexture?> textureFactory )
		{
			mRenderMaterialFactory = materialFactory;
			mRenderTextureFactory = textureFactory;
		}

		public static void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			mRenderMaterialFactory = null;
			mRenderTextureFactory = null;

			Plugins.UnregisterPluginCollector<AssetPluginCollector>();
			Plugins.UnregisterDependency( "Elegy.AssetSystem" );

			mTextures.Clear();
			mMaterialDefs.Clear();
		}
	}
}
