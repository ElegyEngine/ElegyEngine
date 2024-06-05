// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces.Rendering;
using Elegy.Common.Assets;
using Veldrid;

namespace Elegy.RenderSystem.Resources
{
	public class RenderTexture : ITexture
	{
		private GraphicsDevice mDevice;

		public RenderTexture( GraphicsDevice device, in TextureMetadata data, in Span<byte> bytes )
		{
			mDevice = device;
			DeviceTexture = device.ResourceFactory.CreateTexture( new()
			{
				Width = Math.Max( 1, data.Width ),
				Height = Math.Max( 1, data.Height ),
				Depth = Math.Max( 1, data.Depth ),

				ArrayLayers = 1,
				// TODO: mipmapping
				MipLevels = 1,

				// TODO: Make this more complete and rigorous
				Format = data.Compression switch
				{
					TextureCompression.Dxt1 => (data.Components, data.Srgb) switch
					{
						(3, false) => PixelFormat.BC1_Rgb_UNorm,
						(3, true) => PixelFormat.BC1_Rgb_UNorm_SRgb,
						(4, false) => PixelFormat.BC1_Rgba_UNorm,
						(4, true) => PixelFormat.BC1_Rgba_UNorm_SRgb,
						_ => throw new NotSupportedException()
					},

					TextureCompression.Dxt5 => data.Srgb switch
					{
						true => PixelFormat.BC3_UNorm_SRgb,
						false => PixelFormat.BC3_UNorm
					},

					TextureCompression.None => (data.BytesPerPixel, data.Components, data.Srgb, data.Float) switch
					{
						(1, 4, false, false) => PixelFormat.R8_G8_B8_A8_UNorm,
						(1, 4, true, false) => PixelFormat.R8_G8_B8_A8_UNorm_SRgb,

						(2, 1, false, true) => PixelFormat.R16_Float,
						(2, 2, false, true) => PixelFormat.R16_G16_Float,
						(2, 4, false, true) => PixelFormat.R16_G16_B16_A16_Float,

						(4, 1, false, true) => PixelFormat.R32_Float,
						(4, 2, false, true) => PixelFormat.R32_G32_Float,
						(4, 4, false, true) => PixelFormat.R32_G32_B32_A32_Float,

						(4, 1, false, false) => PixelFormat.R32_UInt,
						(4, 2, false, false) => PixelFormat.R32_G32_UInt,
						(4, 4, false, false) => PixelFormat.R32_G32_B32_A32_UInt,

						_ => throw new NotSupportedException()
					},

					_ => throw new NotImplementedException()
				},

				SampleCount = TextureSampleCount.Count1,
				Type = data.Is1D switch
				{
					true => TextureType.Texture1D,
					false => data.Is2D switch
					{
						true => TextureType.Texture2D,
						false => TextureType.Texture3D
					}
				},
				Usage = TextureUsage.Sampled
			} );

			UpdatePixels( bytes );
		}

		public int Width => (int)DeviceTexture.Width;
		public int Height => (int)DeviceTexture.Height;
		public int Depth => (int)DeviceTexture.Depth;

		public Texture DeviceTexture { get; set; }

		public Span<byte> ReadPixels()
		{
			return Array.Empty<byte>();
		}

		public void UpdatePixels( Span<byte> newPixels )
		{
			mDevice.UpdateTexture( DeviceTexture, newPixels, 0, 0, 0, DeviceTexture.Width, DeviceTexture.Height, DeviceTexture.Depth, 0, 0 );
		}

		public void Dispose()
		{
			DeviceTexture.Dispose();
		}
	}
}
