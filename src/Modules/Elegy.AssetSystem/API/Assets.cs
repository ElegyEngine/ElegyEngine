// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.AssetSystem.Loaders;
using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.Common.Interfaces.Rendering;

namespace Elegy.AssetSystem.API
{
	public static partial class Assets
	{
		private static bool mInitialised;

		private static readonly IAssetIo[] mBuiltinLoaders =
		[
			new GltfModelLoader(), // .gltf support
			new ObjModelLoader(), // .obj support. It's not implemented but oh well
			new ElfLevelLoader(), new ElfLevelWriter(), // .elf support (Elegy Level Format)
			new StbTextureLoader() // .png, .jpg, .bmp, .tga support
		];

		public static bool Init()
		{
			mLogger.Log( "Init" );

			foreach ( var assetLoader in mBuiltinLoaders )
			{
				RegisterLoader( assetLoader );
			}

			mInitialised = true;
			return true;
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
			if ( !mInitialised )
			{
				return;
			}

			mLogger.Log( "Shutdown" );

			mRenderMaterialFactory = null;
			mRenderTextureFactory = null;

			mTextures.Clear();
			mMaterialDefs.Clear();
			mInitialised = false;
		}

		public static bool RegisterLoader( IPlugin plugin )
			=> plugin switch
			{
				IModelLoader model => RegisterModelLoader( model ),
				ITextureLoader texture => RegisterTextureLoader( texture ),
				ILevelLoader level => RegisterLevelLoader( level ),
				ILevelWriter levelWriter => RegisterLevelWriter( levelWriter ),
				_ => false
			};

		public static bool UnregisterLoader( IPlugin plugin )
			=> plugin switch
			{
				IModelLoader model => UnregisterModelLoader( model ),
				ITextureLoader texture => UnregisterTextureLoader( texture ),
				ILevelLoader level => UnregisterLevelLoader( level ),
				ILevelWriter levelWriter => UnregisterLevelWriter( levelWriter ),
				_ => false
			};
	}
}
