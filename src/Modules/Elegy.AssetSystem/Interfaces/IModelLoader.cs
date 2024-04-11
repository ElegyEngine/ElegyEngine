// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.AssetSystem.Interfaces
{
	/// <summary>
	/// Model loader interface.
	/// </summary>
	public interface IModelLoader : IAssetLoader
	{
		/// <summary>
		/// Loads a model from the given path.
		/// </summary>
		/// <returns>The model with its data,
		/// <c>null</c> if it cannot be loaded.</returns>
		Model? LoadModel( string path );
	}
}
