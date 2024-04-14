// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.Interfaces;
using Elegy.Common.Assets;
using Elegy.ConsoleSystem;

namespace Elegy.AssetSystem.Loaders
{
	/// <summary>
	/// Built-in OBJ loader.
	/// </summary>
	public class PngImageLoader : BaseAssetLoader, ITextureLoader
	{
		private TaggedLogger mLogger = new( "PngLoader" );

		/// <inheritdoc/>
		public override string Name => "PngImageLoader";

		/// <inheritdoc/>
		public override bool CanLoad( string path )
			=> path == ".png";

		/// <inheritdoc/>
		public (TextureMetadata?, byte[]?) LoadTexture( string path, bool withoutData, bool hintSrgb )
		{
			TextureMetadata? metadata = null;
			byte[]? data = null;

			using ( var stream = File.OpenRead( path ) )
			{
				StbImageSharp.ImageInfo? info = StbImageSharp.Decoding.PngDecoder.Info( stream );
				if ( info is null )
				{
					mLogger.Error( $"'{path}' is not a valid PNG file" );
					return (null, null);
				}

				metadata = new()
				{
					Width = (uint)info.Value.Width,
					Height = (uint)info.Value.Height,
					Depth = 0, // Is always a 2D texture
					BytesPerPixel = (uint)info.Value.BitsPerChannel / 4,
					Components = info.Value.ColorComponents switch
					{
						StbImageSharp.ColorComponents.GreyAlpha => 2U,
						StbImageSharp.ColorComponents.RedGreenBlue => 3U,
						StbImageSharp.ColorComponents.RedGreenBlueAlpha => 4U,
						_ => 1U
					},
					Compression = TextureCompression.None,
					Float = false, // StbImage doesn't do floats it seems
					Srgb = hintSrgb
				};

				if ( !withoutData )
				{
					data = StbImageSharp.Decoding.PngDecoder.Decode( stream ).Data;
				}
			}

			return (metadata, data);
		}
	}
}
