// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;

namespace Elegy.AssetSystem.Interfaces
{
	/// <summary>
	/// Texture loader interface.
	/// </summary>
	public interface ITextureLoader : IAssetLoader
	{
		/// <summary>
		/// Loads a texture from the given <paramref name="path"/>.
		/// </summary>
		/// <param name="path">Full path to an image.</param>
		/// <param name="withoutData">If true, only load the metadata.</param>
		(TextureMetadata?, byte[]?) LoadTexture( string path, bool withoutData );
	}
}
