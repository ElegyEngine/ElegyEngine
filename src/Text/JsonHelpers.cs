// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Text.Json;

namespace Elegy.Text
{
	public static class JsonHelpers
	{
		private static JsonSerializerOptions Options = new()
		{
			AllowTrailingCommas = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true
		};

		public static bool LoadFrom<T>( ref T outObject, string path ) where T : struct
		{
			try
			{
				ReadOnlySpan<byte> jsonContent = File.ReadAllBytes( path );
				outObject = JsonSerializer.Deserialize<T>( jsonContent, Options );
			}
			catch ( Exception ex )
			{
				Console.Error( $"Failed to read '{path}'" );
				Console.Error( $"Message: {ex.Message}" );
				return false;
			}

			return true;
		}

		public static bool Write<T>( T what, string path )
		{
			try
			{
				string jsonString = JsonSerializer.Serialize( what, Options );
				File.WriteAllText( path, jsonString );
			}
			catch ( Exception ex )
			{
				Console.Error( $"Failed to save '{path}'" );
				Console.Error( $"Message: {ex.Message}" );
				return false;
			}

			return true;
		}
	}
}
