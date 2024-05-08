// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Maths
{
	/// <summary>
	/// Elegy's coordinate system.
	/// </summary>
	public static partial class Coords
	{
		/// <summary> The global up vector. </summary>
		public static readonly Vector3 Up = Vector3.UnitZ;

		/// <summary> The global right vector. </summary>
		public static readonly Vector3 Right = Vector3.UnitX;

		/// <summary> The global forward vector. </summary>
		public static readonly Vector3 Forward = Vector3.UnitY;

		/// <summary> The global down vector. </summary>
		public static readonly Vector3 Down = -Vector3.UnitZ;

		/// <summary> The global left vector. </summary>
		public static readonly Vector3 Left = -Vector3.UnitX;

		/// <summary> The global back vector. </summary>
		public static readonly Vector3 Back = -Vector3.UnitY;

		/// <summary> One full turn i.e. 360 degrees. </summary>
		public static readonly float Turn = MathF.PI * 2.0f;

		/// <summary> One full turn upwards i.e. about the <see cref="Right"/> axis. </summary>
		public static readonly Vector3 TurnUp = Vector3.UnitX * MathF.PI * 2.0f;

		/// <summary> One full turn to the right i.e. about the <see cref="Up"/> axis. </summary>
		public static readonly Vector3 TurnRight = Vector3.UnitY * MathF.PI * 2.0f;

		/// <summary> One full twist clockwise i.e. a turn about the <see cref="Forward"/> axis. </summary>
		public static readonly Vector3 TurnTwistRight = Vector3.UnitZ * MathF.PI * 2.0f;

		/// <summary> One full turn downwards i.e. about the <see cref="Right"/> axis in reverse. </summary>
		public static readonly Vector3 TurnDown = -TurnUp;

		/// <summary> One full turn to the left i.e. about the <see cref="Up"/> axis in reverse. </summary>
		public static readonly Vector3 TurnLeft = -TurnRight;

		/// <summary> One full twist counterclockwise i.e. about the <see cref="Forward"/> axis in reverse. </summary>
		public static readonly Vector3 TurnTwistLeft = -TurnTwistRight;
	}
}
