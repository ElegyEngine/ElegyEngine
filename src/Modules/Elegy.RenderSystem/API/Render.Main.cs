// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.PlatformSystem.API;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Interfaces;
using Silk.NET.Windowing;
using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		/// <summary>
		/// What to render every frame.
		/// </summary>
		public static event Action? OnRender;

		/// <summary>
		/// The style to render the world with.
		/// </summary>
		public static IRenderStyle? RenderStyle = new RenderStyleDefault();

		/// <summary>
		/// The graphics device. Updates buffers, swaps stuff, all kinds of GPU things.
		/// </summary>
		public static GraphicsDevice Device => mDevice;

		/// <summary>
		/// The graphics resource factory. Creates buffers, shaders and all kinds of stuff.
		/// </summary>
		public static ResourceFactory Factory => mDevice.ResourceFactory;

		/// <summary>
		/// Renders everything for a given window.
		/// </summary>
		/// <param name="window">The window to render into.</param>
		public static void RenderFrame( IWindow window )
		{
			View? view = GetView( window );
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
		public static void RenderFrame( View view )
		{
			// Start up the counters etc.
			BeginFrame();

			// Render meshes, effects etc.
			OnRender?.Invoke();

			// What was rendered above is now inside a framebuffer
			// Render that framebuffer into one of the framebuffers in the swapchain
			RenderView( view );

			// Finish stuff and present to the screen
			EndFrame();
			PresentView( view );
		}

		/// <summary>
		/// Updates all <see cref="MeshEntity"/>, <see cref="View"/> etc.
		/// instances if their render data changed.
		/// </summary>
		public static void UpdateBuffers()
		{
			mRenderCommands.Begin();

			RebuildDebugMeshes( mRenderCommands );

			foreach ( var meshEntity in mEntitySet )
			{
				meshEntity.UpdateBuffers( mRenderCommands );
			}

			foreach ( var view in mViews )
			{
				view.UpdateBuffers( mRenderCommands );
			}

			mRenderCommands.End();
			mDevice.SubmitCommands( mRenderCommands );
		}

		/// <summary>
		/// Creates a window to render into.
		/// </summary>
		public static View? GetOrCreateDefaultView( int width, int height, int rate = 60 )
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

			if ( GetView( window ) is View view )
			{
				return view;
			}

			View? renderView = CreateView( window );
			if ( renderView is null )
			{
				mLogger.Error( "Cannot create renderview from window!" );
			}

			return renderView;
		}

		public static GraphicsDevice? CreateGraphicsDevice( string[] extraInstanceExtensions, string[] extraDeviceExtensions )
		{
			GraphicsDevice? device = null;

			// Combine the launch config's extensions with the render style's
			List<string> instanceExtensions = mAdditionalInstanceExtensions.ToList();
			List<string> deviceExtensions = mAdditionalDeviceExtensions.ToList();
			instanceExtensions.AddRange( extraInstanceExtensions );
			deviceExtensions.AddRange( extraDeviceExtensions );

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
						InstanceExtensions = instanceExtensions.ToArray(),
						DeviceExtensions = deviceExtensions.ToArray()
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
