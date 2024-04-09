// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Elegy.PluginSystem.API;

namespace Elegy.AssetSystem.API
{
	public static partial class Assets
	{
		public static bool Init( in LaunchConfig config )
		{
			Plugins.RegisterPluginCollector( new AssetPluginCollector() );
			Plugins.RegisterDependency( "Elegy.AssetSystem", typeof( Assets ).Assembly );

			return InitMaterials();
		}

		public static void SetRenderFactories( Func<MaterialDefinition, IMaterial> materialFactory, Func<TextureMetadata, byte[], ITexture> textureFactory )
		{
			mRenderMaterialFactory = materialFactory;
			mRenderTextureFactory = textureFactory;
		}

		public static void Shutdown()
		{
			mTextures.Clear();
			mMaterialDefs.Clear();
		}
	}
}
