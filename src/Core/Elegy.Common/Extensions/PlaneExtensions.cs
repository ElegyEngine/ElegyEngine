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

			// Right is +X
			if ( normal.X >= normal.Y && normal.X >= normal.Z )
			{
				return Coords.Right;
			}

			// Forward is +Y
			if ( normal.Y >= normal.Z )
			{
				return Coords.Forward;
			}

			// Up is +Z
			return Coords.Up;
		}

		/// <summary>
		/// Gets the closest axis to the plane's normal. This is for <see cref="Assets.BrushMapData.Brush"/> faces
		/// that do not have paraxial texture coordinates.
		/// </summary>
		public static Vector3D GetClosestAxis( this PlaneD plane )
		{
			Vector3D normal = plane.Normal.Abs();

			// Forward is +Y
			if ( normal.Y >= normal.X && normal.Y >= normal.Z )
			{
				return Vector3D.UnitY;
			}

			// Right is +X
			if ( normal.X >= normal.Z )
			{
				return Vector3D.UnitX;
			}

			// Up is +Z
			return Vector3D.UnitZ;
		}
	}
}
