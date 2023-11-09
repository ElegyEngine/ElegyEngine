// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Text.JsonAdapters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elegy.Text
{
	/// <summary>
	/// JSON reading and writing utilities.
	/// </summary>
	public static class JsonHelpers
	{
		private readonly static JsonSerializerOptions Options = new()
		{
			AllowTrailingCommas = true,
			NumberHandling =
				JsonNumberHandling.AllowNamedFloatingPointLiterals |
				JsonNumberHandling.AllowReadingFromString,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,

			Converters =
			{
				new GodotVector4Converter(),
				new GodotVector3Converter(),
				new GodotVector2Converter(),
				new GodotAabbConverter(),
				new GodotRect2Converter()
			}
		};

		/// <summary>
		/// Reads JSON data from <paramref name="path"/> and stores it into
		/// <paramref name="outObject"/>.
		/// </summary>
		/// <returns><c>true</c> on success, <c>false</c> if the file cannot be found.</returns>
		public static bool LoadFrom<T>( ref T outObject, string path ) where T : struct
		{
			try
			{
				ReadOnlySpan<byte> jsonContent = File.ReadAllBytes( path );
				outObject = JsonSerializer.Deserialize<T>( jsonContent, Options );
			}
			catch ( DirectoryNotFoundException )
			{
				return false;
			}
			catch ( FileNotFoundException )
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads JSON data from <paramref name="path"/> and returns it as a/an <typeparamref name="T"/>.
		/// </summary>
		/// <returns><c>true</c> on success, <c>false</c> if the file cannot be found.</returns>
		public static T? LoadFrom<T>( string path ) where T : class
		{
			try
			{
				ReadOnlySpan<byte> jsonContent = File.ReadAllBytes( path );
				return JsonSerializer.Deserialize<T>( jsonContent, Options );
			}
			catch ( DirectoryNotFoundException )
			{
				return null;
			}
			catch ( FileNotFoundException )
			{
				return null;
			}
		}

		/// <summary>
		/// Writes <paramref name="what"/> into a file <paramref name="path"/>.
		/// </summary>
		/// <returns><c>true</c> on success, <c>false</c> if the path is invalid.</returns>
		public static bool Write<T>( T what, string path )
		{
			try
			{
				string jsonString = JsonSerializer.Serialize( what, Options );
				File.WriteAllText( path, jsonString );
			}
			catch ( DirectoryNotFoundException )
			{
				return false;
			}

			return true;
		}
	}
}
