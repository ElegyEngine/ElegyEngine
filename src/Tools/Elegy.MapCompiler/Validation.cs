// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Elegy.MapCompiler
{
	internal class GeoValidation
	{
		public static bool Scalar( float x, string message )
		{
			bool isNan = x == float.NaN;
			bool isInfinite = float.IsInfinity( x );

			Debug.Assert( !isNan, $"{message} - got value 'NaN'" );
			Debug.Assert( !isInfinite, $"{message} - got value 'infinity'" );

			return !isNan && !isInfinite;
		}

		public static bool Vec3( Vector3 v, string message )
		{
			return Scalar( v.X, $"{message} - X" )
				&& Scalar( v.Y, $"{message} - Y" )
				&& Scalar( v.Z, $"{message} - Z" );
		}
	}
}
