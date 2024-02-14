// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Diagnostics;

namespace Elegy
{
	internal class CoreInternal
	{
		private Stopwatch mStopwatch;
		private IWindowPlatform? mWindowPlatform;
		private List<IWindow> mWindows = [];
		private List<IInputContext> mInputContexts = [];
		private IWindow mFocusWindow;
		private IInputContext mFocusInputContext;

		private readonly IWindow mDummyWindow = new WindowNull();
		private readonly IInputContext mDummyInput = new InputContextNull();

		internal CoreInternal( Stopwatch sw, IWindowPlatform? windowPlatform )
		{
			mFocusWindow = mDummyWindow;
			mFocusInputContext = mDummyInput;

			mWindowPlatform = windowPlatform;
			mStopwatch = sw;
			sw.Restart();
		}

		public bool AddWindow( IWindow window )
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

		public bool HasWindow( IWindow window )
		{
			return mWindows.Contains( window );
		}

		public bool RemoveWindow( IWindow window )
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
						mFocusWindow = mDummyWindow;
						mFocusInputContext = mDummyInput;
					}

					return true;
				}
			}

			return false;
		}

		public IWindow? CreateWindow( in WindowOptions options )
		{
			if ( mWindowPlatform is null )
			{
				return null;
			}

			IWindow window = mWindowPlatform.CreateWindow( options );
			AddWindow( window );

			return window;
		}

		public IWindow GetCurrentWindow()
		{
			return mFocusWindow;
		}

		public IInputContext GetCurrentInputContext()
		{
			return mFocusInputContext;
		}

		public long GetTicks() => mStopwatch.ElapsedTicks;

		public double GetSeconds() => mStopwatch.ElapsedTicks / Stopwatch.Frequency;

		public bool IsHeadless { get; internal set; } = false;
	}
}
