// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Elegy.PlatformSystem.API
{
	public static partial class Platform
	{
		/// <summary>
		/// Associates the <paramref name="window"/> with the engine, and
		/// returns <c>false</c> if it's already associated.
		/// </summary>
		public static bool AddWindow( IWindow window )
		{
			if ( mWindows.Contains( window ) )
			{
				return false;
			}

			var doRegisterWindow = () =>
			{
				IInputContext inputContext = window.CreateInput();

				mWindows.Add( window );
				mInputContexts.Add( inputContext );

				mFocusWindow = window;
				mFocusInputContext = inputContext;

				window.FocusChanged += ( newState ) =>
				{
					if ( newState && HasWindow( window ) )
					{
						mFocusWindow = window;
						mFocusInputContext = inputContext;
					}
				};

				window.Closing += () => RemoveWindow( window );
			};

			// In case a window is not yet initialised (just created), create an
			// input context etc. on load, when the window is actually ready to do so
			if ( !window.IsInitialized )
			{
				window.Load += doRegisterWindow;
				window.Initialize();
				window.IsVisible = true;
				return true;
			}

			doRegisterWindow();
			return true;
		}

		/// <summary>
		/// Returns whether the <paramref name="window"/> is associated with the engine.
		/// </summary>
		public static bool HasWindow( IWindow window )
		{
			return mWindows.Contains( window );
		}

		/// <summary>
		/// Disassociates a window from the engine, and returns
		/// <c>false</c> if it's not found in the first place.
		/// </summary>
		public static bool RemoveWindow( IWindow window )
		{
			for ( int i = 0; i < mWindows.Count; i++ )
			{
				if ( mWindows[i] == window )
				{
					mInputContexts[i].Dispose();
					mInputContexts.RemoveAt( i );
					mWindows.RemoveAt( i );

					if ( mFocusWindow == window )
					{
						mFocusWindow = null;
						mFocusInputContext = mDummyInput;
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Creates a window, or returns <c>null</c> in case there's no window platform (dedicated server etc.)
		/// </summary>
		public static IWindow? CreateWindow( in WindowOptions options )
		{
			if ( mWindowPlatform is null )
			{
				return null;
			}

			IWindow window = mWindowPlatform.CreateWindow( options );
			AddWindow( window );

			return window;
		}

		/// <summary>
		/// Returns the window that is currently in focus, or <c>null</c> if there are no windows.
		/// </summary>
		/// <returns></returns>
		public static IWindow? GetCurrentWindow()
		{
			return mFocusWindow;
		}

		/// <summary>
		/// Returns the currently focused window's input context, or <c>null</c> if there are no windows.
		/// </summary>
		public static IInputContext GetCurrentInputContext()
		{
			return mFocusInputContext;
		}
	}
}
