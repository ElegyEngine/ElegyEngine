// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Core.Contexts;
using Silk.NET.Windowing;
using Veldrid;

namespace Elegy.RenderBackend
{
	public static class WindowSurfaceHelper
	{
		public static Swapchain CreateSwapchain( GraphicsDevice device, IView window )
		{
			if ( window.Native is null )
			{
				throw new ArgumentNullException();
			}

			SwapchainDescription desc = new()
			{
				Source = GetSwapchainSource( window.Native ),
				Width = (uint)window.Size.X,
				Height = (uint)window.Size.Y,
				ColorSrgb = false,
				DepthFormat = null,
				SyncToVerticalBlank = window.VSync
			};

			return device.ResourceFactory.CreateSwapchain( desc );
		}

		public static SwapchainSource GetSwapchainSource( INativeWindow view )
		{
			if ( view.Win32.HasValue )
			{
				return SwapchainSource.CreateWin32( view.Win32.Value.Hwnd, view.Win32.Value.HInstance );
			}

			if ( view.X11.HasValue )
			{
				return SwapchainSource.CreateXlib( view.X11.Value.Display, (nint)view.X11.Value.Window );
			}

			if ( view.Wayland.HasValue )
			{
				return SwapchainSource.CreateWayland( view.Wayland.Value.Display, view.Wayland.Value.Surface );
			}

			// TODO: Android support
			// I wanna make Elegy apps for my VR headset!!!

			throw new PlatformNotSupportedException();
		}
	}
}
