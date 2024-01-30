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
		/// Seconds elapsed since engine start.
		/// </summary>
		public static double Seconds => mCoreSystem.GetSeconds();
	}
}
