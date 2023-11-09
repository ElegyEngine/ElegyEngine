
using System.Globalization;

namespace Elegy.Utilities
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
		/// Attempts to parse a float from a string, modifying <c>result</c> upon success.
		/// </summary>
		public static bool TryFloat( string token, out float result )
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
