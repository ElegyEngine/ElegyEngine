// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Elegy.PlatformSystem.API
{
	/// <summary>
	/// Platforming system. Handles windowing and provides access to input.
	/// </summary>
	public static partial class Platform
	{
		public static bool Init()
		{
			mWindowPlatform = null;

			mLogger.Log( "Init" );

			return true;
		}

		public static void Set( IWindowPlatform? windowPlatform )
		{
			mWindowPlatform = windowPlatform;
		}

		public static void OverrideInput( IInputContext inputContext )
		{
			mFocusInputContext = inputContext;
		}

		public static void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			Set( null );
			OverrideInput( mDummyInput );
		}
	}
}
