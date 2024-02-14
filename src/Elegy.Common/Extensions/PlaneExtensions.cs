// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Maths;

namespace Elegy.Extensions
{
	public static class PlaneExtensions
	{
		public static Vector3 GetClosestAxis( this Plane plane )
		{
			Vector3 normal = plane.Normal.Abs();

			if ( normal.Z >= normal.X && normal.Z >= normal.Y )
			{
				return Vector3.UnitY;
			}

			if ( normal.X >= normal.Y )
			{
				return Vector3.UnitX;
			}

			return Vector3.UnitZ;
		}

		public static Vector3D GetClosestAxis( this PlaneD plane )
		{
			Vector3D normal = plane.Normal.Abs();

			if ( normal.Z >= normal.X && normal.Z >= normal.Y )
			{
				return Vector3D.UnitY;
			}

			if ( normal.X >= normal.Y )
			{
				return Vector3D.UnitX;
			}

			return Vector3D.UnitZ;
		}
	}
}
