// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.Interfaces
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
		/// <param name="width">Output width in pixels.</param>
		/// <param name="height">Output height in pixels.</param>
		/// <param name="depth">If it's a 3D texture, how many layers.</param>
		/// <param name="bytes">The output pixel bytes.</param>
		/// <returns><see langword="true"/> upon success, <see langword="false"/> otherwise.</returns>
		bool LoadTexture( string path, out int width, out int height, out int depth, out Span<byte> bytes );
	}
}
