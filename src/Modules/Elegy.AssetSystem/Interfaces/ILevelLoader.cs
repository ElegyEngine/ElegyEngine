// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.AssetSystem.Interfaces
{
	/// <summary>
	/// Level loader interface.
	/// </summary>
	public interface ILevelLoader : IAssetIo
	{
		/// <summary>
		/// 
		/// </summary>
		ElegyMapDocument? LoadLevel( string path );
	}
}
