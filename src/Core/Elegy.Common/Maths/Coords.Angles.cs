﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

// NOTE: This contains code from BepuUtilities:
// https://github.com/bepu/bepuphysics2/blob/master/BepuUtilities/QuaternionEx.cs
// In particular, the methods: TransformUnitX, TransformUnitY and TransformUnitZ.

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
				CosYaw  * -SinRoll + SinYaw * SinPitch * CosRoll,
				-SinYaw * -SinRoll + CosYaw * SinPitch * CosRoll,
				CosPitch * CosRoll
			);
		}

		public static void DirectionsFromQuaternion( Quaternion orientation, out Vector3 outForward, out Vector3 outUp )
		{
			outForward = QuaternionUnitY( orientation );
			outUp = QuaternionUnitZ( orientation );
		}

		public static Vector3 QuaternionUnitX( Quaternion orientation )
		{
			Vector3 result;
			float y2 = orientation.Y + orientation.Y;
			float z2 = orientation.Z + orientation.Z;
			float xy2 = orientation.X * y2;
			float xz2 = orientation.X * z2;
			float yy2 = orientation.Y * y2;
			float zz2 = orientation.Z * z2;
			float wy2 = orientation.W * y2;
			float wz2 = orientation.W * z2;
			result.X = 1f  - yy2 - zz2;
			result.Y = xy2 + wz2;
			result.Z = xz2 - wy2;
			return result;
		}

		public static Vector3 QuaternionUnitY( Quaternion orientation )
		{
			Vector3 result;
			float x2 = orientation.X + orientation.X;
			float y2 = orientation.Y + orientation.Y;
			float z2 = orientation.Z + orientation.Z;
			float xx2 = orientation.X * x2;
			float xy2 = orientation.X * y2;
			float yz2 = orientation.Y * z2;
			float zz2 = orientation.Z * z2;
			float wx2 = orientation.W * x2;
			float wz2 = orientation.W * z2;
			result.X = xy2 - wz2;
			result.Y = 1f  - xx2 - zz2;
			result.Z = yz2 + wx2;
			return result;
		}

		public static Vector3 QuaternionUnitZ( Quaternion orientation )
		{
			Vector3 result;
			float x2 = orientation.X + orientation.X;
			float y2 = orientation.Y + orientation.Y;
			float z2 = orientation.Z + orientation.Z;
			float xx2 = orientation.X * x2;
			float xz2 = orientation.X * z2;
			float yy2 = orientation.Y * y2;
			float yz2 = orientation.Y * z2;
			float wx2 = orientation.W * x2;
			float wy2 = orientation.W * y2;
			result.X = xz2 + wy2;
			result.Y = yz2 - wx2;
			result.Z = 1f  - xx2 - yy2;
			return result;
		}

		public static Quaternion QuaternionFromAxisAngle( Vector3 axis, float angle )
		{
			double halfAngle = angle * 0.5;
			double s = Math.Sin( halfAngle );
			Quaternion q;
			q.X = (float)(axis.X * s);
			q.Y = (float)(axis.Y * s);
			q.Z = (float)(axis.Z * s);
			q.W = (float)Math.Cos( halfAngle );
			return q;
		}

		public static Quaternion QuaternionFromDegrees( Vector3 eulerAngles )
			=> QuaternionFromRadians( eulerAngles * Deg2Rad );

		public static Quaternion QuaternionFromRadians( Vector3 eulerAngles )
		{
			Quaternion q;
			double halfRoll = eulerAngles.Z  * 0.5;
			double halfPitch = eulerAngles.X * 0.5;
			double halfYaw = eulerAngles.Y   * 0.5;

			double sinRoll = Math.Sin( halfRoll );
			double sinPitch = Math.Sin( halfPitch );
			double sinYaw = Math.Sin( halfYaw );

			double cosRoll = Math.Cos( halfRoll );
			double cosPitch = Math.Cos( halfPitch );
			double cosYaw = Math.Cos( halfYaw );

			double cosYawCosPitch = cosYaw * cosPitch;
			double cosYawSinPitch = cosYaw * sinPitch;
			double sinYawCosPitch = sinYaw * cosPitch;
			double sinYawSinPitch = sinYaw * sinPitch;

			q.X = (float)(cosYawSinPitch * cosRoll + sinYawCosPitch * sinRoll);
			q.Y = (float)(sinYawCosPitch * cosRoll - cosYawSinPitch * sinRoll);
			q.Z = (float)(cosYawCosPitch * sinRoll - sinYawSinPitch * cosRoll);
			q.W = (float)(cosYawCosPitch * cosRoll + sinYawSinPitch * sinRoll);
			return q;
		}

		public static Quaternion WorldQuaternionFromDegrees( Vector3 eulerAngles )
			=> WorldQuaternionFromRadians( eulerAngles * Deg2Rad );

		public static Quaternion WorldQuaternionFromRadians( Vector3 eulerAngles )
		{
			Quaternion qyaw = QuaternionFromAxisAngle( Up, -eulerAngles.Y );
			Quaternion qpitch = QuaternionFromAxisAngle( Right, eulerAngles.X );
			Quaternion qyawpitch = Quaternion.Multiply( qyaw, qpitch );
			Quaternion qroll = QuaternionFromAxisAngle( Forward, eulerAngles.Z );

			return Quaternion.Multiply( qyawpitch, qroll );
		}
	}
}
