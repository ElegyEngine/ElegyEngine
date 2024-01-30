// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Engine core. Keeps track of time.
	/// </summary>
	public static partial class Core
	{
		/// <summary>
		/// Ticks elapsed since engine start.
		/// </summary>
		public static long Ticks => mCoreSystem.GetTicks();

		/// <summary>
		/// Ticks elapsed since engine start.
		/// </summary>
		public static int TicksInt => (int)Ticks;

		/// <summary>
		/// Seconds elapsed since engine start.
		/// </summary>
		public static double Seconds => mCoreSystem.GetSeconds();

		/// <summary>
		/// Seconds elapsed since engine start.
		/// </summary>
		public static float SecondsFloat => (float)Seconds;
	}
}
