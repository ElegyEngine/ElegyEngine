// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.AssetSystem.Interfaces
{
	/// <summary>
	/// Level writer interface.
	/// </summary>
	public interface ILevelWriter : IAssetIo
	{
		/// <summary>
		/// 
		/// </summary>
		bool WriteLevel( string path, ElegyMapDocument map );
	}
}
