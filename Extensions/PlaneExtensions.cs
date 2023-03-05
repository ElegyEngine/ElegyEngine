// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.Extensions
{
	public static class PlaneExtensions
	{
		public static Vector3 GetClosestAxis( this Plane plane )
		{
			Vector3 normal = plane.Normal.Abs();

			if ( normal.Z >= normal.X && normal.Z >= normal.Y )
			{
				return Vector3.Forward;
			}

			if ( normal.X >= normal.Y )
			{
				return Vector3.Right;
			}

			return Vector3.Up;
		}
	}
}
