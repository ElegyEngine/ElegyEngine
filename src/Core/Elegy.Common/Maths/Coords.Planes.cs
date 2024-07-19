// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Maths
{
	/// <summary>
	/// Elegy's coordinate system.
	/// </summary>
	public static partial class Coords
	{
		public static Plane PlaneFromPoints( Vector3 a, Vector3 b, Vector3 c )
		{
			Vector3 normal = Vector3.Normalize( Vector3.Cross( c - a, b - a ) );
			return new( normal, Vector3.Dot( normal, a ) );
		}
	}
}
