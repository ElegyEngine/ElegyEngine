// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Elegy.PlatformSystem.API
{
	public static partial class Platform
	{
		private readonly static IWindow mDummyWindow = new WindowNull();
		private readonly static IInputContext mDummyInput = new InputContextNull();

		private static IWindowPlatform? mWindowPlatform;
		private static List<IWindow> mWindows = [];
		private static List<IInputContext> mInputContexts = [];
		private static IWindow mFocusWindow = mDummyWindow;
		private static IInputContext mFocusInputContext = mDummyInput;
	}
}
