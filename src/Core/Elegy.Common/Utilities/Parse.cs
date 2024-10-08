﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace Elegy.Common.Utilities
{
	/// <summary>
	/// Numeric parsing utilities.
	/// </summary>
	public static class Parse
	{
		/// <summary>
		/// Attempts to parse an integer from a string, modifying <c>result</c> upon success.
		/// </summary>
		public static bool TryInt( string token, out int result )
		{
			return int.TryParse( token, NumberStyles.Integer, CultureInfo.InvariantCulture, out result );
		}

		/// <summary>
		/// Parses an integer from a string.
		/// </summary>
		/// <param name="token">Something like "20".</param>
		/// <returns>A valid integer value on successful parse, 0 if the token is invalid.</returns>
		public static int Int( string token )
		{
			if ( TryInt( token, out int result ) )
			{
				return result;
			}

			return 0;
		}

		/// <summary>
		/// Attempts to parse a float from a string, modifying <paramref name="result"/> upon success.
		/// </summary>
		public static bool TryFloat( string token, out float result )
		{
			return float.TryParse( token, NumberStyles.Float, CultureInfo.InvariantCulture, out result );
		}

		/// <summary>
		/// Attempts to parse a float from a string, modifying <paramref name="result"/> upon success.
		/// </summary>
		public static bool TryFloat( ReadOnlySpan<char> token, out float result )
		{
			return float.TryParse( token, NumberStyles.Float, CultureInfo.InvariantCulture, out result );
		}

		/// <summary>
		/// Parses a float from a string.
		/// </summary>
		/// <param name="token">Something like "20.05".</param>
		/// <returns>A valid floating-point value on successful parse, 0.0f if the token is invalid.</returns>
		public static float Float( string token )
		{
			if ( TryFloat( token, out float result ) )
			{
				return result;
			}

			return 0.0f;
		}

		/// <summary>
		/// Parses a float from a string.
		/// </summary>
		/// <param name="token">Something like "20.05".</param>
		/// <returns>A valid floating-point value on successful parse, 0.0f if the token is invalid.</returns>
		public static float Float( ReadOnlySpan<char> token )
		{
			if ( TryFloat( token, out float result ) )
			{
				return result;
			}

			return 0.0f;
		}

		/// <summary>
		/// Parses a <see cref="System.Numerics.Vector2"/> from a string.
		/// </summary>
		/// <param name="token">A string like "10 20".</param>
		public static Vector2 Vector2( string token )
		{
			ReadOnlySpan<char> xstring = token.AsSpan( 0, token.IndexOf( ' ' ) );
			ReadOnlySpan<char> ystring = token.AsSpan( token.IndexOf( ' ' ) + 1 );

			return new()
			{
				X = Float( xstring ),
				Y = Float( ystring )
			};
		}

		/// <summary>
		/// Parses a <see cref="System.Numerics.Vector3"/> from a string.
		/// </summary>
		/// <param name="token">A string like "10 20 30".</param>
		public static Vector3 Vector3( string token )
		{
			int firstSpace = token.IndexOf( ' ' );
			int secondSpace = token.IndexOf( ' ', firstSpace + 1 );

			ReadOnlySpan<char> xstring = token.AsSpan( 0, firstSpace );
			ReadOnlySpan<char> ystring = token.AsSpan( firstSpace + 1, secondSpace - firstSpace );
			ReadOnlySpan<char> zstring = token.AsSpan( secondSpace );

			return new()
			{
				X = Float( xstring ),
				Y = Float( ystring ),
				Z = Float( zstring )
			};
		}

		// TODO: DateOnly instead of DateTime?

		/// <summary>
		/// Attempts to parse a <see cref="DateTime"/> from a string, modifying <c>result</c> upon success.
		/// </summary>
		public static bool TryDate( string token, out DateTime result )
		{
			return DateTime.TryParseExact( token, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result );
		}

		/// <summary>
		/// Parses dates in dd/MM/yyyy format.
		/// </summary>
		public static DateTime Date( string token )
		{
			if ( TryDate( token, out DateTime result ) )
			{
				return result;
			}

			return DateTime.MinValue;
		}
	}
}
