// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Assets
{
	/// <summary>
	/// Texture compression type.
	/// </summary>
	public enum TextureCompression
	{
		None,
		Dxt1,
		Dxt5,
		Uastc,
		Etc1s,
	}

	/// <summary>Represents info about the texture, without its actual data.</summary>
	public class TextureMetadata
	{
		/// <summary> Width in pixels. </summary>
		public uint Width { get; init; }
		/// <summary> Height in pixels. </summary>
		public uint Height { get; init; }
		/// <summary> Depth in pixels. </summary>
		public uint Depth { get; init; }
		/// <summary> Components per pixel (RGB, grayscale, RGBA...) </summary>
		public uint Components { get; init; } = 4;
		/// <summary> Number of bytes per pixel. </summary>
		public uint BytesPerPixel { get; init; } = 1;
		/// <summary> Is this a floating-point texture? </summary>
		public bool Float { get; init; } = false;
		/// <summary> Type of texture compression, if any. </summary>
		public TextureCompression Compression { get; init; } = TextureCompression.None;

		/// <summary> Is this a 1D texture? </summary>
		public bool Is1D => Height == 0 && Depth == 0;
		/// <summary> Is this a 2D texture? </summary>
		public bool Is2D => Depth == 0;
		/// <summary> Is this a 3D texture? </summary>
		public bool Is3D => !Is2D;
	}
}
