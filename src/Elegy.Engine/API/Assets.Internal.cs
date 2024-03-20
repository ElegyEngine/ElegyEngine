// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.API
{
	public static partial class Assets
	{
		internal static void SetAssetSystem( AssetSystemInternal? system )
			=> mAssetSystem = system;

		private static AssetSystemInternal? mAssetSystem;
	}
}
