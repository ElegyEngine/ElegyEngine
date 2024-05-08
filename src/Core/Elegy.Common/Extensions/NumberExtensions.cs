// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Extensions
{
	/// <summary>
	/// Extensions for floats and doubles.
	/// </summary>
	public static class NumberExtensions
	{
		/// <summary> Returns this radian angle in degrees. </summary>
		public static float ToDegrees( this float x )
			=> x * Coords.Rad2Deg;

		/// <summary> Returns this degree angle in radians. </summary>
		public static float ToRadians( this float x )
			=> x * Coords.Deg2Rad;

		/// <summary> Returns this radian angle in degrees. </summary>
		public static double ToDegrees( this double x )
			=> x * Coords.Rad2DegD;

		/// <summary> Returns this degree angle in radians. </summary>
		public static double ToRadians( this double x )
			=> x * Coords.Deg2RadD;
	}
}
