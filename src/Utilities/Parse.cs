
using System.Globalization;

namespace Elegy.Utilities
{
	public static class Parse
	{
		public static bool TryInt( string token, out int result )
		{
			return int.TryParse( token, NumberStyles.Integer, CultureInfo.InvariantCulture, out result );
		}

		public static int Int( string token )
		{
			if ( TryInt( token, out int result ) )
			{
				return result;
			}

			return 0;
		}

		public static bool TryFloat( string token, out float result )
		{
			return float.TryParse( token, NumberStyles.Float, CultureInfo.InvariantCulture, out result );
		}

		public static float Float( string token )
		{
			if ( TryFloat( token, out float result ) )
			{
				return result;
			}

			return 0.0f;
		}

		// TODO: DateOnly instead of DateTime?

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
