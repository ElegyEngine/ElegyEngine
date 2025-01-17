using System.Diagnostics;
using Silk.NET.OpenXR;
using Silk.NET.OpenXR.Extensions.KHR;
using TerraFX.Interop.Vulkan;
using Veldrid;
using Veldrid.Vulkan;

namespace XrResearch
{
	public static partial class Program
	{
		private static long mPredictedDisplayTime;
		private static CompositionLayerProjection mLayerProjection = new() { Type = StructureType.CompositionLayerProjection };
		private static List<CompositionLayerProjectionView> mLayerProjectionViews = new();
		private static List<IntPtr> mLayers = new(); // CompositionLayerBaseHeader*
		private static ViewConfigurationView[] mConfigurationViews;
		private static View[] mViews;

		private static IVrGraphicsApi? mGraphicsApi;
		private static SwapchainInfo mColourSwapchainInfo;
		private static SwapchainInfo mDepthSwapchainInfo;

		private static unsafe void GetViewConfigurationViews()
		{
			Console.WriteLine( "GetViewConfigurationViews" );

			uint viewCount = 0;
			ViewConfigurationType viewType = ViewConfigurationType.PrimaryStereo;
			XrCheck( Xr.EnumerateViewConfigurationView( mInstance, mSystemId, viewType, 0, ref viewCount, null ) );

			mConfigurationViews = new ViewConfigurationView[viewCount];
			Array.Fill( mConfigurationViews, new() { Type = StructureType.ViewConfigurationView } );
			XrCheck( Xr.EnumerateViewConfigurationView( mInstance, mSystemId, viewType, ref viewCount, mConfigurationViews.AsSpan() ) );

			mViews = new View[viewCount];
			Array.Fill( mViews, new() { Type = StructureType.View } );

			// Check if they're all coherent
			uint firstWidth = GetVrSwapchainWidth();
			uint firstHeight = GetVrSwapchainHeight();

			for ( int i = 1; i < viewCount; i++ )
			{
				uint width = mConfigurationViews[i].RecommendedImageRectWidth;
				uint height = mConfigurationViews[i].RecommendedImageRectHeight;
				if ( width != firstWidth || height != firstHeight )
				{
					Console.WriteLine( $"WARNING: Incoherent resolution for view {i}: {width}x{height} vs. {firstWidth}x{firstHeight}" );
				}
			}
		}

		private static unsafe void CreateSwapchains()
		{
			Console.WriteLine( "CreateSwapchains" );

			if ( mGraphicsApi is null )
			{
				return;
			}

			// Get the supported swapchain formats as an array of int64_t and ordered by runtime preference.
			uint formatCount = 0;
			;
			XrCheck( Xr.EnumerateSwapchainFormats( mSession, 0, ref formatCount, null ), "Failed to enumerate Swapchain Formats" );
			long[] formats = new long[formatCount];
			XrCheck( Xr.EnumerateSwapchainFormats( mSession, ref formatCount, formats.AsSpan() ), "Failed to enumerate Swapchain Formats" );
			if ( mGraphicsApi.SelectDepthSwapchainFormat( formats ) == 0L )
			{
				Console.WriteLine( "WARNING: Failed to find depth format for swapchain" );
				Debugger.Break();
			}

			//Resize the SwapchainInfo to match the number of view in the View Configuration.
			mColourSwapchainInfo = new();
			mDepthSwapchainInfo = new();

			mColourSwapchainInfo.SwapchainFormat = mGraphicsApi.SelectColorSwapchainFormat( formats );
			mDepthSwapchainInfo.SwapchainFormat = mGraphicsApi.SelectDepthSwapchainFormat( formats );

			// Fill out an XrSwapchainCreateInfo structure and create an XrSwapchain.
			// Color.
			SwapchainCreateInfo swapchainCi = new()
			{
				Type = StructureType.SwapchainCreateInfo,
				CreateFlags = SwapchainCreateFlags.None,
				UsageFlags = SwapchainUsageFlags.SampledBit | SwapchainUsageFlags.ColorAttachmentBit,
				Format = mColourSwapchainInfo.SwapchainFormat,
				SampleCount = mConfigurationViews[0].RecommendedSwapchainSampleCount,
				Width = GetVrSwapchainWidth(),
				Height = GetVrSwapchainHeight(),
				FaceCount = 1,
				ArraySize = (uint)GetVrArrayLayerCount(),
				MipCount = 1
			};

			XrCheck( Xr.CreateSwapchain( mSession, swapchainCi, ref mColourSwapchainInfo.Swapchain ),
				"Failed to create Color Swapchain" );

			// Depth.
			swapchainCi = new()
			{
				Type = StructureType.SwapchainCreateInfo,
				CreateFlags = SwapchainCreateFlags.None,
				UsageFlags = SwapchainUsageFlags.SampledBit | SwapchainUsageFlags.DepthStencilAttachmentBit,
				Format = mDepthSwapchainInfo.SwapchainFormat,
				SampleCount = mConfigurationViews[0].RecommendedSwapchainSampleCount,
				Width = GetVrSwapchainWidth(),
				Height = GetVrSwapchainHeight(),
				FaceCount = 1,
				ArraySize = (uint)GetVrArrayLayerCount(),
				MipCount = 1
			};

			XrCheck( Xr.CreateSwapchain( mSession, swapchainCi, ref mDepthSwapchainInfo.Swapchain ),
				"Failed to create Depth Swapchain" );

			// Get the number of images in the color/depth swapchain and allocate
			// Swapchain image data via GraphicsAPI to store the returned array
			uint colorSwapchainImageCount = 0;
			XrCheck( Xr.EnumerateSwapchainImages( mColourSwapchainInfo.Swapchain, 0, ref colorSwapchainImageCount, null ),
				"Failed to enumerate Color Swapchain Images." );

			Span<SwapchainImageBaseHeader> colorSwapchainImages =
				mGraphicsApi.AllocateSwapchainImageData( mColourSwapchainInfo.Swapchain, colorSwapchainImageCount, depth: false );
			XrCheck( Xr.EnumerateSwapchainImages(
					mColourSwapchainInfo.Swapchain, ref colorSwapchainImageCount, colorSwapchainImages ),
				"Failed to enumerate Color Swapchain Images." );

			uint depthSwapchainImageCount = 0;
			XrCheck( Xr.EnumerateSwapchainImages( mDepthSwapchainInfo.Swapchain, 0, &depthSwapchainImageCount, null ),
				"Failed to enumerate Depth Swapchain Images." );

			Span<SwapchainImageBaseHeader> depthSwapchainImages =
				mGraphicsApi.AllocateSwapchainImageData( mDepthSwapchainInfo.Swapchain, depthSwapchainImageCount, depth: true );
			XrCheck( Xr.EnumerateSwapchainImages(
					mDepthSwapchainInfo.Swapchain, depthSwapchainImageCount, &depthSwapchainImageCount, depthSwapchainImages ),
				"Failed to enumerate Depth Swapchain Images." );

			mColourSwapchainInfo.ImageViews = new( (int)colorSwapchainImageCount );
			mDepthSwapchainInfo.ImageViews = new( (int)depthSwapchainImageCount );

			// Per image in the swapchains, create a color/depth image view
			for ( uint j = 0; j < colorSwapchainImageCount; j++ )
			{
				ImageViewCreateInfo imageViewCi = new()
				{
					Image = mGraphicsApi.CreateSwapchainImage( mColourSwapchainInfo.Swapchain, j, depth: false ),
					IsDepth = false,
					Format = mColourSwapchainInfo.SwapchainFormat,
					LayerIndex = j,
					LayerCount = GetVrArrayLayerCount()
				};
				mColourSwapchainInfo.ImageViews.Add( mGraphicsApi.CreateImageView( imageViewCi ) );

				imageViewCi = new()
				{
					Image = mGraphicsApi.CreateSwapchainImage( mDepthSwapchainInfo.Swapchain, j, depth: true ),
					IsDepth = true,
					Format = mDepthSwapchainInfo.SwapchainFormat,
					LayerIndex = j,
					LayerCount = GetVrArrayLayerCount()
				};
				mDepthSwapchainInfo.ImageViews.Add( mGraphicsApi.CreateImageView( imageViewCi ) );
			}

			mGraphicsApi.CreateFramebuffers( mColourSwapchainInfo, mDepthSwapchainInfo );
		}

		public static uint GetVrSwapchainWidth()
		{
			ref var view = ref mConfigurationViews[0];
			return view.RecommendedImageRectWidth;
		}

		public static uint GetVrSwapchainHeight()
		{
			ref var view = ref mConfigurationViews[0];
			return view.RecommendedImageRectHeight;
		}

		public static int GetVrArrayLayerCount()
		{
			//return 1;
			return mConfigurationViews.Length;
		}

		public static void InitGraphics()
		{
			Console.WriteLine( "InitGraphics" );

			GetViewConfigurationViews();
			CreateSwapchains();
		}

		public static void AddGraphicsExtensions()
		{
			mInstanceExtensions.Add( KhrVulkanEnable.ExtensionName );
			mInstanceExtensions.Add( "XR_MND_headless" );
		}

		public static void ShutdownGraphics()
		{
			Console.WriteLine( "ShutdownGraphics" );

			if ( mGraphicsApi is null )
			{
				return;
			}

			mGraphicsApi.DestroyFramebuffers();

			// Destroy the color and depth image views from GraphicsAPI.
			foreach ( object imageView in mColourSwapchainInfo.ImageViews )
			{
				mGraphicsApi.DestroyImageView( imageView );
			}

			foreach ( object imageView in mDepthSwapchainInfo.ImageViews )
			{
				mGraphicsApi.DestroyImageView( imageView );
			}

			// Free the Swapchain Image Data.
			mGraphicsApi.FreeSwapchainImageData( mColourSwapchainInfo.Swapchain );
			mGraphicsApi.FreeSwapchainImageData( mDepthSwapchainInfo.Swapchain );

			// Destroy the swapchains.
			XrCheck( Xr.DestroySwapchain( mColourSwapchainInfo.Swapchain ), "Failed to destroy Color Swapchain" );
			XrCheck( Xr.DestroySwapchain( mDepthSwapchainInfo.Swapchain ), "Failed to destroy Depth Swapchain" );
		}

		public static unsafe bool RenderLayer( uint shouldRender )
		{
			if ( shouldRender == 0 || mGraphicsApi is null )
			{
				return false;
			}

			ViewState viewState = new() { Type = StructureType.ViewState };
			// Will contain information on whether the position and/or orientation is valid and/or tracked.
			ViewLocateInfo viewLocateInfo = new() { Type = StructureType.ViewLocateInfo };

			viewLocateInfo.ViewConfigurationType = ViewConfigurationType.PrimaryStereo;
			viewLocateInfo.DisplayTime = mPredictedDisplayTime;
			viewLocateInfo.Space = mReferenceSpace;
			uint viewCount = 0;
			if ( !XrCheck( Xr.LocateView( mSession, viewLocateInfo, ref viewState, ref viewCount, mViews.AsSpan() ) ) )
			{
				Console.WriteLine( "Failed to locate Views" );
				return false;
			}

			// Resize the layer projection views to match the view count. The layer projection views are used in the layer projection.
			mLayerProjectionViews.Clear();
			mLayerProjectionViews.EnsureCapacity( (int)viewCount );

			// Acquire and wait for an image from the swapchains.
			// Get the image index of an image in the swapchains.
			// The timeout is infinite.
			uint colorImageIndex = 0;
			uint depthImageIndex = 0;

			SwapchainImageAcquireInfo acquireInfo = new() { Type = StructureType.SwapchainImageAcquireInfo };
			XrCheck( Xr.AcquireSwapchainImage( mColourSwapchainInfo.Swapchain, acquireInfo, ref colorImageIndex ),
				"Failed to acquire Image from the Color Swapchain" );
			XrCheck( Xr.AcquireSwapchainImage( mDepthSwapchainInfo.Swapchain, acquireInfo, ref depthImageIndex ),
				"Failed to acquire Image from the Depth Swapchain" );

			SwapchainImageWaitInfo waitInfo = new()
			{
				Type = StructureType.SwapchainImageWaitInfo,
				Timeout = 0x7fffffffffffffffL // XR_INFINITE_DURATION
			};

			XrCheck( Xr.WaitSwapchainImage( mColourSwapchainInfo.Swapchain, waitInfo ),
				"Failed to wait for Image from the Color Swapchain" );
			XrCheck( Xr.WaitSwapchainImage( mDepthSwapchainInfo.Swapchain, waitInfo ),
				"Failed to wait for Image from the Depth Swapchain" );

			// Per view in the view configuration:
			for ( int i = 0; i < viewCount; i++ )
			{
				// Fill out the XrCompositionLayerProjectionView structure specifying the pose and fov from the view.
				// This also associates the swapchain image with this layer projection view.
				mLayerProjectionViews.Add( new()
				{
					Type = StructureType.CompositionLayerProjectionView,
					Pose = mViews[i].Pose,
					Fov = mViews[i].Fov,
					SubImage = new()
					{
						Swapchain = mColourSwapchainInfo.Swapchain,
						ImageArrayIndex = (uint)i, // Useful for multiview rendering.
						ImageRect = new()
						{
							Offset = new( 0, 0 ),
							Extent = new( (int)GetVrSwapchainHeight(), (int)GetVrSwapchainHeight() )
						}
					}
				} );
			}

			// Get the width and height and construct the viewport and scissors.
			//uint width = GetVrSwapchainHeight();
			//uint height = GetVrSwapchainHeight();
			//GraphicsAPI::Viewport viewport = { 0.0f, 0.0f, (float)width, (float)height, 0.0f, 1.0f };
			//GraphicsAPI::Rect2D scissor = { { (int32_t)0, (int32_t)0 }, { width, height } };
			//float nearZ = 0.05f;
			//float farZ = 100.0f;

			Framebuffer fb = (Framebuffer)mGraphicsApi.GetFramebuffer( mColourSwapchainInfo.Swapchain, colorImageIndex );

			mCommands.Begin();
			mCommands.SetFramebuffer( fb );
			mCommands.ClearColorTarget( 0, RgbaFloat.Grey );
			mCommands.End();
			mDevice.SubmitCommands( mCommands );
			mDevice.WaitForIdle();

			//mGraphicsApi->BeginRendering();

			// TODO: Actually render stuff

			//mGraphicsApi->EndRendering();

			// Give the swapchain image back to OpenXR, allowing the compositor to use the image.
			SwapchainImageReleaseInfo releaseInfo = new() { Type = StructureType.SwapchainImageReleaseInfo };
			XrCheck( Xr.ReleaseSwapchainImage( mColourSwapchainInfo.Swapchain, &releaseInfo ),
				"Failed to release Image back to the Color Swapchain" );
			XrCheck( Xr.ReleaseSwapchainImage( mDepthSwapchainInfo.Swapchain, &releaseInfo ),
				"Failed to release Image back to the Depth Swapchain" );

			// Fill out the XrCompositionLayerProjection structure for usage with xrEndFrame().
			mLayerProjection.LayerFlags = CompositionLayerFlags.BlendTextureSourceAlphaBit;
			mLayerProjection.Space = mReferenceSpace;
			mLayerProjection.ViewCount = (uint)mLayerProjectionViews.Count;
			mLayerProjection.Views = mLayerProjectionViews.Deref();

			return true;
		}

		public static unsafe void RenderFrame()
		{
			FrameState frameState = new() { Type = StructureType.FrameState };
			FrameWaitInfo frameWaitInfo = new() { Type = StructureType.FrameWaitInfo };
			if ( !XrCheck( Xr.WaitFrame( mSession, frameWaitInfo, ref frameState ), "WaitFrame error", true ) )
			{
				return;
			}

			FrameBeginInfo frameBeginInfo = new() { Type = StructureType.FrameBeginInfo };
			VkGraphicsDevice.ShouldThrowOnValidationError = false;
			XrCheck( Xr.BeginFrame( mSession, frameBeginInfo ), "BeginFrame error", true );
			VkGraphicsDevice.ShouldThrowOnValidationError = true;

			bool sessionFocused = mSessionState is SessionState.Visible or SessionState.Focused;
			bool sessionActive = sessionFocused || mSessionState is SessionState.Synchronized;

			mLayers.Clear();
			mPredictedDisplayTime = frameState.PredictedDisplayTime;

			if ( sessionActive )
			{
				if ( sessionFocused )
				{
					PollActions( frameState.PredictedDisplayTime );
				}

				// TODO: Actually render stuff
				if ( RenderLayer( frameState.ShouldRender ) )
				{
					fixed ( CompositionLayerProjection* ptr = &mLayerProjection )
					{
						mLayers.Add( (IntPtr)(CompositionLayerBaseHeader*)ptr );
					}
				}
			}

			FrameEndInfo frameEndInfo = new()
			{
				Type = StructureType.FrameEndInfo,
				DisplayTime = frameState.PredictedDisplayTime,
				EnvironmentBlendMode = EnvironmentBlendMode.Opaque,
				LayerCount = (uint)mLayers.Count,
				Layers = mLayers.DerefDouble<CompositionLayerBaseHeader>()
			};
			// SteamVR trips Vulkan validation errors which causes Veldrid to throw and mess everything up
			VkGraphicsDevice.ShouldThrowOnValidationError = false;
			XrCheck( Xr.EndFrame( mSession, frameEndInfo ), "EndFrame error", true );
			VkGraphicsDevice.ShouldThrowOnValidationError = true;
		}
	}
}
