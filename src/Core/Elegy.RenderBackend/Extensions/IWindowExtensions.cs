// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Windowing;

namespace Elegy.RenderBackend.Extensions
{
	public static class IWindowExtensions
	{
		public static bool CanSwap( this IWindow window )
		{
			if ( window.IsClosing )
			{
				return false;
			}

			if ( window.WindowState == WindowState.Minimized )
			{
				return false;
			}

			return window.IsVisible;
		}
	}
}
