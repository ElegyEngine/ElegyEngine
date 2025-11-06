// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Resources;
using Elegy.RenderBackend.Extensions;

using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

using ITexture = Elegy.Common.Interfaces.Rendering.ITexture;

namespace Elegy.RenderSystem.Objects
{
	/// <summary>
	/// A renderable view.
	/// </summary>
	public class View : IDisposable
	{
		[StructLayout( LayoutKind.Sequential )]
		private struct CameraData
		{
			// TODO: Handle multiview CameraData
			public Matrix4x4 Transform;
			public Matrix4x4 Projection;
		}

		private readonly GraphicsDevice mDevice;
		private bool mHasExternalSwapchain;
		private CameraData mCameraData;

		private View( GraphicsDevice device )
		{
			mDevice = device;
			CameraBuffer = mDevice.ResourceFactory
				.CreateBufferForStruct<CameraData>( BufferUsage.UniformBuffer );

			PerViewSet = mDevice.ResourceFactory.CreateSet( Render.Layouts.PerView, CameraBuffer );

			mDevice.UpdateBuffer( CameraBuffer, 0, mCameraData );
		}

		// TODO: View needs more clear constructors: window swapchains, composition swapchains, texture targets

		internal View( GraphicsDevice device, ITexture renderTarget )
			: this( device )
		{
			Debug.Assert( renderTarget is RenderTexture );
			TargetTexture = ((RenderTexture)renderTarget).DeviceTexture;

			ViewTexture = mDevice.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
				width: (uint)renderTarget.Width,
				height: (uint)renderTarget.Height,
				mipLevels: 1,
				arrayLayers: 1,
				format: PixelFormat.B8_G8_R8_A8_UNorm,
				usage: TextureUsage.RenderTarget | TextureUsage.Sampled ) );

			DepthTexture = mDevice.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
				width: (uint)renderTarget.Width,
				height: (uint)renderTarget.Height,
				mipLevels: 1,
				arrayLayers: 1,
				format: PixelFormat.D32_Float_S8_UInt,
				usage: TextureUsage.DepthStencil ) );

			BackBuffer = mDevice.ResourceFactory.CreateFramebuffer( new( null, TargetTexture ) );
			RenderBuffer = mDevice.ResourceFactory.CreateFramebuffer( new( DepthTexture, ViewTexture ) );
			RenderSize = new( renderTarget.Width, renderTarget.Height );

			WindowSet = mDevice.ResourceFactory.CreateSet( Render.Layouts.Window, ViewTexture, Render.Samplers.LinearBorder );
		}

		internal View( GraphicsDevice device, IWindow window, Swapchain? externalSwapchain )
			: this( device )
		{
			Window = window;
			TargetSwapchain = externalSwapchain ?? RenderBackend.WindowSurfaceHelper.CreateSwapchain( device, window );
			mHasExternalSwapchain = externalSwapchain != null;

			RegenerateWindowResources( false );

			window.FramebufferResize += newSize =>
			{
				TargetSwapchain.Resize( (uint)newSize.X, (uint)newSize.Y );
				RegenerateWindowResources( true );
			};
		}

		/// <summary>
		/// Called when this view is being rendered.
		/// </summary>
		public Action<CommandList> OnRender;

		/// <summary>
		/// The render mask.
		/// </summary>
		public int Mask { get; set; } = int.MinValue;

		/// <summary>
		/// The native window associated with this view.
		/// </summary>
		public IWindow? Window { get; private set; }

		/// <summary>
		/// Render output size in pixels.
		/// </summary>
		public Vector2I RenderSize { get; set; }

		/// <summary>
		/// Framebuffer associated with <see cref="TargetSwapchain"/> for displaying the view onto the window.
		/// In the case of a custom render target, it's just a framebuffer to perform post-processing on.
		/// </summary>
		public Framebuffer BackBuffer { get; private set; }

		/// <summary>
		/// The texture associated with either the <see cref="Window"/> or a custom render target.
		/// Doesn't have a depth buffer, meant to be a target of post-processing.
		/// </summary>
		public Texture? TargetTexture { get; private set; }

		/// <summary>
		/// The swapchain associated with the <see cref="Window"/>. If that is null, this is null too.
		/// </summary>
		public Swapchain? TargetSwapchain { get; private set; }

		/// <summary>
		/// Framebuffer associated with <see cref="ViewTexture"/> and <see cref="DepthTexture"/> for rendering.
		/// </summary>
		public Framebuffer RenderBuffer { get; private set; }

		/// <summary>
		/// The texture to render into before post-processing and displaying it on the window.
		/// </summary>
		public Texture ViewTexture { get; set; }

		/// <summary>
		/// Depth buffer for depth testing.
		/// </summary>
		public Texture DepthTexture { get; set; }

		/// <summary>
		/// <see cref="ViewTexture"/> + a sampler object.
		/// </summary>
		public ResourceSet WindowSet { get; set; }

		/// <summary>
		/// Transformation matrix of this view.
		/// </summary>
		public Matrix4x4 Transform
		{
			get => mCameraData.Transform;
			set
			{
				mCameraData.Transform = value;
				TransformOrProjectionChanged = true;
			}
		}

		/// <summary>
		/// Projection matrix of this view.
		/// </summary>
		public Matrix4x4 Projection
		{
			get => mCameraData.Projection;
			set
			{
				mCameraData.Projection = value;
				TransformOrProjectionChanged = true;
			}
		}

		internal bool TransformOrProjectionChanged { get; set; }
		public DeviceBuffer CameraBuffer { get; private set; }
		public ResourceSet PerViewSet { get; private set; }

		/// <summary>
		/// Updates buffers on the GPU.
		/// </summary>
		public void UpdateBuffers( CommandList commands )
		{
			if ( TransformOrProjectionChanged )
			{
				commands.UpdateBuffer( CameraBuffer, 0, mCameraData );
				TransformOrProjectionChanged = false;
			}
		}

		/// <summary>
		/// Regenerates the view's resources.
		/// </summary>
		public void RegenerateWindowResources( bool dispose )
		{
			if ( Window is null || TargetSwapchain is null )
			{
				return;
			}

			BackBuffer = TargetSwapchain.Framebuffer;
			TargetTexture = BackBuffer.ColorTargets[0].Target;

			if ( dispose )
			{
				WindowSet.Dispose();
				RenderBuffer.Dispose();
				DepthTexture.Dispose();
				ViewTexture.Dispose();
			}

			ViewTexture = mDevice.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
				width: TargetTexture.Width,
				height: TargetTexture.Height,
				mipLevels: 1,
				arrayLayers: 1,
				format: PixelFormat.B8_G8_R8_A8_UNorm,
				usage: TextureUsage.RenderTarget | TextureUsage.Sampled ) );

			DepthTexture = mDevice.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
				width: TargetTexture.Width,
				height: TargetTexture.Height,
				mipLevels: 1,
				arrayLayers: 1,
				format: PixelFormat.D32_Float_S8_UInt,
				usage: TextureUsage.DepthStencil ) );

			RenderBuffer = mDevice.ResourceFactory.CreateFramebuffer( new( DepthTexture, ViewTexture ) );
			RenderSize = new( Window.Size.X, Window.Size.Y );

			WindowSet = mDevice.ResourceFactory.CreateSet( Render.Layouts.Window, ViewTexture, Render.Samplers.LinearBorder );
		}

		/// <summary>
		/// Disposes of all resources.
		/// </summary>
		public void Dispose()
		{
			PerViewSet.Dispose();
			WindowSet.Dispose();
			RenderBuffer.Dispose();

			ViewTexture.Dispose();
			DepthTexture.Dispose();
			CameraBuffer.Dispose();
		}
	}
}
