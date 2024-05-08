// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.AssetSystem.Interfaces.Rendering;

namespace Elegy.AssetSystem.Resources
{
	/// <summary></summary>
	public class Texture
	{
		/// <summary></summary>
		public Texture( TextureMetadata metadata )
		{
			Metadata = metadata;
		}

		/// <summary></summary>
		public static byte[] GenerateMissingTexturePattern( int width, int height )
		{
			int Stride = width * 4;

			// Based on: https://github.com/Admer456/learnin-nvrhi/blob/master/src/Texture.cpp#L119
			byte[] textureBytes = new byte[width * height * 4];
			for ( int y = 0; y < height; y++ )
			{
				for ( int x = 0; x < width; x++ )
				{
					int index = y * Stride + x * 4;

					// Here it's a bit swapped compared to the original, because of RGBA vs. BGRA differences
					textureBytes[index + 0] = (byte)(50.0f + 50.0f * MathF.Sin( (x + y) / 5.0f ));
					textureBytes[index + 1] = (byte)(60.0f + 50.0f * MathF.Sin( y / 5.0f ));
					textureBytes[index + 2] = (byte)(50.0f - 40.0f * MathF.Sin( x / 5.0f ));
					textureBytes[index + 3] = 255;

					if ( (y % 4 == 0) || (x % 4 == 0) )
					{
						textureBytes[index + 0] += 20;
						textureBytes[index + 1] += 20;
						textureBytes[index + 2] += 20;
					}
				}
			}

			return textureBytes;
		}

		/// <summary></summary>
		public TextureMetadata Metadata { get; init; }

		/// <summary></summary>
		public ITexture? RenderTexture { get; init; } = null;
	}
}
