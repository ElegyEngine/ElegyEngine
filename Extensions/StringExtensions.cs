// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Extensions
{
	public static class StringExtensions
	{
		public static bool IsNumeric( this char c )
		{
			return c >= '0' && c <= '9';
		}

		public static float ToFloat( this string token )
		{
			return float.Parse( token, System.Globalization.CultureInfo.InvariantCulture );
		}
	}
}
