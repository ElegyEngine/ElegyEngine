// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine;
using Elegy.Engine.API;

namespace Elegy.Engine
{
	internal partial class AssetSystemInternal
	{
		private TaggedLogger mLogger = new( "AssetSystem" );

		public bool Init()
		{
			mLogger.Log( "Init" );

			Assets.SetAssetSystem( this );

			return InitMaterials();
		}

		public void Shutdown()
		{
			mTextures.Clear();
			mMaterialDefs.Clear();
		}
	}
}
