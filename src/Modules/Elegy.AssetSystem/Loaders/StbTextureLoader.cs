// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Formats.Asn1;
using Elegy.AssetSystem.Interfaces;
using Elegy.Common.Assets;
using Elegy.Common.Utilities;
using StbImageSharp;
using StbImageSharp.Decoding;

namespace Elegy.AssetSystem.Loaders
{
	/// <summary>
	/// Built-in OBJ loader.
	/// </summary>
	public class StbTextureLoader : BaseAssetIo, ITextureLoader
	{
		private TaggedLogger mLogger = new( "PngLoader" );

		/// <inheritdoc/>
		public override string Name => "PngImageLoader";

		/// <inheritdoc/>
		public override bool Supports( string path )
			=> path is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tga";

		/// <inheritdoc/>
		public (TextureMetadata?, byte[]?) LoadTexture( string path, bool withoutData, bool hintSrgb )
		{
			TextureMetadata? metadata;
			byte[]? data = null;

			using ( var stream = File.OpenRead( path ) )
			{
				string extension = Path.GetExtension( path ).ToLower();
				ImageInfo? info = extension switch
				{
					".bmp" => BmpDecoder.Info( stream ),
					".png" => PngDecoder.Info( stream ),
					".tga" => TgaDecoder.Info( stream ),
					_ => JpgDecoder.Info( stream )
				};

				if ( info is null )
				{
					mLogger.Error( $"'{path}' is not a valid {extension[1..].ToUpper()} file" );
					return (null, null);
				}

				metadata = new()
				{
					Width = (uint)info.Value.Width,
					Height = (uint)info.Value.Height,
					Depth = 1, // Is always a 2D texture
					BytesPerPixel = (uint)info.Value.BitsPerChannel / 8,
					Components = info.Value.ColorComponents switch
					{
						ColorComponents.GreyAlpha => 2U,
						ColorComponents.RedGreenBlue => 4U,
						ColorComponents.RedGreenBlueAlpha => 4U,
						_ => 1U
					},
					Compression = TextureCompression.None,
					Float = false, // StbImage doesn't do floats it seems
					Srgb = hintSrgb
				};

				if ( !withoutData )
				{
					// Modern GAPIs do not support RGB; only R, RG and RGBA.
					ColorComponents? requiredComponents =
						info.Value.ColorComponents == ColorComponents.RedGreenBlue
						? ColorComponents.RedGreenBlueAlpha
						: null;

					data = extension switch
					{
						".bmp" => BmpDecoder.Decode( stream, requiredComponents ).Data,
						".png" => PngDecoder.Decode( stream, requiredComponents ).Data,
						".tga" => TgaDecoder.Decode( stream, requiredComponents ).Data,
						_ => JpgDecoder.Decode( stream, requiredComponents ).Data
					};
				}
			}

			return (metadata, data);
		}
	}
}
