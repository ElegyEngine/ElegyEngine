// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Maths
{
	/// <summary>
	/// Elegy's coordinate system.
	/// </summary>
	public static partial class Coords
	{
		/// <summary> Factor that converts degrees into radians. </summary>
		public const float Deg2Rad = MathF.PI / 180.0f;

		/// <summary> Factor that converts radians into degrees. </summary>
		public const float Rad2Deg = 180.0f / MathF.PI;

		/// <summary> Factor that converts degrees into radians. </summary>
		public const double Deg2RadD = MathF.PI / 180.0f;

		/// <summary> Factor that converts radians into degrees. </summary>
		public const double Rad2DegD = 180.0f / MathF.PI;

		/// <summary>
		/// Calculates forward and up vectors from <paramref name="angles"/>, provided in degrees.
		/// You can obtain a right vector by cross-producing forward and up.
		/// </summary>
		public static void DirectionsFromDegrees( Vector3 angles, out Vector3 outForward, out Vector3 outUp )
			=> DirectionsFromRadians( angles * Deg2Rad, out outForward, out outUp );

		/// <summary>
		/// Calculates forward and up vectors from <paramref name="angles"/>, provided in radians.
		/// You can obtain a right vector by cross-producing forward and up.
		/// </summary>
		public static void DirectionsFromRadians( Vector3 angles, out Vector3 outForward, out Vector3 outUp )
		{
			// Based on: https://github.com/Admer456/adm-utils/blob/master/src/Maths/Mat4.cpp#L52
			(float SinPitch, float CosPitch) = MathF.SinCos( -angles.X );
			(float SinYaw, float CosYaw) = MathF.SinCos( angles.Y );
			(float SinRoll, float CosRoll) = MathF.SinCos( -angles.Z );

			// Original is X-forward. X and Y here are swapped so it can be Y-forward
			outForward = new(
				SinYaw * CosPitch,
				CosYaw * CosPitch,
				-SinPitch
			);

			outUp = new(
				CosYaw * -SinRoll + SinYaw * SinPitch * CosRoll,
				-SinYaw * -SinRoll + CosYaw * SinPitch * CosRoll,
				CosPitch * CosRoll
			);
		}
	}
}
