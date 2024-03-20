// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;

namespace Elegy.Common.Extensions
{
	/// <summary>
	/// Elegy-specific <see cref="Plane"/> extensions. Mainly for TrenchBroom map compilation.
	/// </summary>
	public static class PlaneExtensions
	{
		/// <summary>
		/// Gets the closest axis to the plane's normal. This is for <see cref="Assets.BrushMapData.Brush"/> faces
		/// that do not have paraxial texture coordinates.
		/// </summary>
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

		/// <summary>
		/// Gets the closest axis to the plane's normal. This is for <see cref="Assets.BrushMapData.Brush"/> faces
		/// that do not have paraxial texture coordinates.
		/// </summary>
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
