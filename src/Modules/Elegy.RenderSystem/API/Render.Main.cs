// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.PlatformSystem.API;
using Elegy.RenderSystem.Interfaces;
using Silk.NET.Windowing;
using TerraFX.Interop.Vulkan;
using Veldrid;
using IView = Elegy.RenderSystem.Interfaces.Rendering.IView;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		public static IRenderFrontend Instance => mFrontend;

		/// <summary>
		/// Renders everything for a given window.
		/// </summary>
		/// <param name="window">The window to render into.</param>
		public static void RenderFrame( IWindow window )
		{
			IView? view = Render.Instance.GetView( window );
			if ( view is null )
			{
				mLogger.Error( "Cannot render frame - there is no renderview for the window!" );
				return;
			}

			RenderFrame( view );
		}

		/// <summary>
		/// Renders everything for a given view.
		/// </summary>
		/// <param name="view">The view to render from. Will render and update an <see cref="IWindow"/>.</param>
		public static void RenderFrame( IView view )
		{
			Render.Instance.BeginFrame();
			Render.Instance.RenderView( view );
			Render.Instance.EndFrame();
			Render.Instance.PresentView( view );
		}

		/// <summary>
		/// Creates a window to render into.
		/// </summary>
		public static IView? GetOrCreateDefaultView( int width, int height, int rate = 60 )
		{
			IWindow? window = Platform.GetCurrentWindow();
			if ( window is null )
			{
				window = Platform.CreateWindow( new()
				{
					API = GraphicsAPI.DefaultVulkan,
					FramesPerSecond = rate,
					UpdatesPerSecond = rate,
					Size = new( width, height )
				} );

				if ( window is null )
				{
					mLogger.Error( "Cannot create window!" );
					return null;
				}
			}

			if ( Instance.GetView( window ) is IView view )
			{
				return view;
			}

			IView? renderView = Instance.CreateView( window );
			if ( renderView is null )
			{
				mLogger.Error( "Cannot create renderview from window!" );
			}

			return renderView;
		}
		
		public static GraphicsDevice? CreateGraphicsDevice()
		{
			GraphicsDevice? device = null;

			try
			{
				device = GraphicsDevice.CreateVulkan( new()
				{
#if DEBUG
					Debug = true,
#endif
					ResourceBindingModel = ResourceBindingModel.Improved,
					SyncToVerticalBlank = true,

					SwapchainSrgbFormat = false,
					SwapchainDepthFormat = null,

					//PreferDepthRangeZeroToOne = true,
					//PreferStandardClipSpaceYDirection = true,

					// We are gonna create swapchains manually for IViews
					HasMainSwapchain = false
				},

				new VulkanDeviceOptions()
				{
					// Nothing in here for now, though we may want
					// Vulkan 1.3 dynamic state extensions at some point
				} );

			}
			catch ( Exception ex )
			{
				mLogger.Fatal( "Error while creating graphics device" );
				mLogger.Error( $"Exception message: {ex.Message}" );
			}

			return device;
		}
	}
}
