// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Elegy
{
	/// <summary>
	/// Engine core. Keeps track of time, windows and other platform things.
	/// </summary>
	public static partial class Core
	{
		/// <summary>
		/// Associates the <paramref name="window"/> with the engine, and
		/// returns <c>false</c> if it's already associated.
		/// </summary>
		public static bool AddWindow( IWindow window )
			=> mCoreSystem.AddWindow( window );

		/// <summary>
		/// Returns whether the <paramref name="window"/> is associated with the engine.
		/// </summary>
		public static bool HasWindow( IWindow window )
			=> mCoreSystem.HasWindow( window );

		/// <summary>
		/// Disassociates a window from the engine, and returns
		/// <c>false</c> if it's not found in the first place.
		/// </summary>
		public static bool RemoveWindow( IWindow window )
			=> mCoreSystem.RemoveWindow( window );

		/// <summary>
		/// Creates a window, or returns <c>null</c> in case there's no window platform (dedicated server etc.)
		/// </summary>
		public static IWindow? CreateWindow( in WindowOptions options )
			=> mCoreSystem.CreateWindow( options );

		/// <summary>
		/// Returns the window that is currently in focus, or <c>null</c> if there are no windows.
		/// </summary>
		/// <returns></returns>
		public static IWindow GetCurrentWindow()
			=> mCoreSystem.GetCurrentWindow();

		/// <summary>
		/// Returns the currently focused window's input context, or <c>null</c> if there are no windows.
		/// </summary>
		public static IInputContext GetCurrentInputContext()
			=> mCoreSystem.GetCurrentInputContext();

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
