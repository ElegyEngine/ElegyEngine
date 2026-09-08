using System.Diagnostics;

namespace Elegy.Common.Extensions;

/// <summary>
/// Extensions for .NET's Stopwatch.
/// </summary>
public static class StopwatchExtensions
{
	/// <summary>
	/// Shorthand for elapsedTicks / frequency. Double version.
	/// </summary>
	public static double GetSecondsF64( this Stopwatch self )
		=> (double)self.ElapsedTicks / Stopwatch.Frequency;

	/// <summary>
	/// Shorthand for elapsedTicks / frequency. Float version.
	/// </summary>
	public static float GetSecondsF32( this Stopwatch self )
		=> (float)self.ElapsedTicks / Stopwatch.Frequency;

	public static long GetMicroseconds( this Stopwatch self )
		=> self.ElapsedTicks / (Stopwatch.Frequency / 1000);
}
