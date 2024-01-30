// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace Elegy
{
	internal class CoreInternal
	{
		private Stopwatch mStopwatch;

		internal CoreInternal( Stopwatch sw )
		{
			mStopwatch = sw;
			sw.Restart();
		}

		public long GetTicks() => mStopwatch.ElapsedTicks;

		public double GetSeconds() => mStopwatch.ElapsedTicks / Stopwatch.Frequency;
	}
}
