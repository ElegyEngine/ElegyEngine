﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Engine.API;
using Elegy.Engine.Interfaces.Rendering;

namespace Elegy.Engine.Resources
{
	/// <summary></summary>
	public class Texture
	{
		/// <summary></summary>
		public static Texture FromData( TextureMetadata metadata, Span<byte> bytes )
			=> new()
			{
				Metadata = metadata,
				RenderTexture = Render.Instance.CreateTexture( metadata, bytes )
			};

		/// <summary></summary>
		public static byte[] GenerateMissingTexturePattern()
		{
			const int Stride = 16 * 4;

			// Based on: https://github.com/Admer456/learnin-nvrhi/blob/master/src/Texture.cpp#L119
			byte[] textureBytes = new byte[16 * 16 * 4];
			for ( int y = 0; y < 16; y++ )
			{
				for ( int x = 0; x < 16; x++ )
				{
					int index = y * Stride + x * 4;

					textureBytes[index + 3] = 255;

					if ( (y % 4 == 0) || (x % 4 == 0) )
					{
						textureBytes[index] = 240;
						textureBytes[index + 1] = 240;
						textureBytes[index + 2] = 240;
					}
					else
					{
						textureBytes[index] = (byte)(50.0f - 40.0f * MathF.Sin( x / 5.0f ));
						textureBytes[index] = (byte)(60.0f + 50.0f * MathF.Sin( y / 5.0f ));
						textureBytes[index] = (byte)(50.0f + 50.0f * MathF.Sin( (x + y) / 5.0f ));
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
