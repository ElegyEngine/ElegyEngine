// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;

using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Elegy.PlatformSystem.API
{
	public static partial class Platform
	{
		private static TaggedLogger mLogger = new( "Platform" );
		private readonly static IInputContext mDummyInput = new InputContextNull();

		private static IWindowPlatform? mWindowPlatform;
		private static List<IWindow> mWindows = [];
		private static List<IInputContext> mInputContexts = [];
		private static IWindow? mFocusWindow = null;
		private static IInputContext mFocusInputContext = mDummyInput;
	}
}
