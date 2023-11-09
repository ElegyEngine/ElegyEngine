// SPDX-FileCopyrightText: 2023 Admer Šuko
// SPDX-License-Identifier: MIT

// TODO: Replace with Elegy's material system when it has one

using System.Buffers.Binary;

namespace Elegy.MapCompiler.Data.Processing
{
	internal static class ImageLoader
	{
		internal static Material? LoadMaterial( string materialName )
		{
			if ( FileSystem.FileExists( $"{materialName}.png" ) )
			{
				return LoadMaterialFromPng( materialName );
			}
			else if ( FileSystem.FileExists( $"{materialName}.jpeg" ) || FileSystem.FileExists( $"{materialName}.jpg" ) )
			{
				Console.WriteLine( $"[MaterialManager] JPEG not supported yet ('{materialName}.jp[e]g')" );
			}
			else if ( FileSystem.FileExists( $"{materialName}.bmp" ) )
			{
				Console.WriteLine( $"[MaterialManager] BMP not supported yet ('{materialName}.bmp')" );
			}
			else if ( FileSystem.FileExists( $"{materialName}.dds" ) )
			{
				Console.WriteLine( $"[MaterialManager] DDS not supported yet ('{materialName}.dds')" );
			}
			else if ( FileSystem.FileExists( $"{materialName}.qoi" ) )
			{
				Console.WriteLine( $"[MaterialManager] QOI not supported yet ('{materialName}.qoi')" );
			}

			return null;
		}

		private static readonly byte[] PngSignature =
		{// \211  P     N     G     \r    \n    \032  \n
			0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a
		};

		internal static Material? LoadMaterialFromPng( string path )
		{
			byte[] bytes = FileSystem.ReadAllBytes( $"{path}.png" );
			for ( int i = 0; i < PngSignature.Length; i++ )
			{
				if ( bytes[i] != PngSignature[i] )
				{
					return null;
				}
			}

			ByteBuffer buffer = new( bytes );
			buffer.Advance( PngSignature.Length );

			// Let's assume we're at the IHDR chunk now

			// Chunk length
			buffer.ReadI32();
			// Chunk type
			buffer.ReadI32();

			int width = BinaryPrimitives.ReverseEndianness( buffer.ReadI32() );
			int height = BinaryPrimitives.ReverseEndianness( buffer.ReadI32() );

			if ( width <= 0 || height <= 0 )
			{
				Console.WriteLine( $"[MaterialManager] Invalid dimensions '{width}x{height}' ('{path}')" );
				return null;
			}

			Console.WriteLine( $"[MaterialManager] Loaded {width}x{height} ('{path}.png')" );

			return new()
			{
				Width = width,
				Height = height
			};
		}
	}
}
