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

using ITexture = Elegy.AssetSystem.Interfaces.Rendering.ITexture;

namespace Elegy.RenderSystem.Objects
{
	public class View : IDisposable
	{
		[StructLayout( LayoutKind.Sequential )]
		private struct CameraData
		{
			public Matrix4x4 Transform;
			public Matrix4x4 Projection;
		}

		private CameraData mCameraData;

		private View( GraphicsDevice device )
		{
			CameraBuffer = device.ResourceFactory
				.CreateBufferForStruct<CameraData>( BufferUsage.UniformBuffer );

			PerViewSet = device.ResourceFactory.CreateSet( Render.Layouts.PerView, CameraBuffer );
		}

		internal View( GraphicsDevice device, ITexture renderTarget )
			: this( device )
		{
			Debug.Assert( renderTarget is RenderTexture );
			TargetTexture = ((RenderTexture)renderTarget).DeviceTexture;

			ViewTexture = device.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
				width: (uint)renderTarget.Width,
				height: (uint)renderTarget.Height,
				mipLevels: 1,
				arrayLayers: 1,
				format: PixelFormat.B8_G8_R8_A8_UNorm,
				usage: TextureUsage.RenderTarget | TextureUsage.Sampled ) );

			DepthTexture = device.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
				width: (uint)renderTarget.Width,
				height: (uint)renderTarget.Height,
				mipLevels: 1,
				arrayLayers: 1,
				format: PixelFormat.D32_Float_S8_UInt,
				usage: TextureUsage.DepthStencil ) );

			Framebuffer = device.ResourceFactory.CreateFramebuffer( new( null, TargetTexture ) );
			RenderSize = new( renderTarget.Width, renderTarget.Height );

			WindowSet = device.ResourceFactory.CreateSet( Render.Layouts.Window, ViewTexture, Render.Samplers.LinearBorder );
		}

		internal View( GraphicsDevice device, IWindow window )
			: this( device )
		{
			Window = window;
			TargetSwapchain = RenderBackend.WindowSurfaceHelper.CreateSwapchain( device, window );

			var regenerateBuffers = ( bool dispose ) =>
			{
				Framebuffer = TargetSwapchain.Framebuffer;
				TargetTexture = Framebuffer.ColorTargets[0].Target;

				if ( dispose )
				{
					WindowSet.Dispose();
					RenderFramebuffer.Dispose();
					DepthTexture.Dispose();
					ViewTexture.Dispose();
				}

				ViewTexture = device.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
					width: TargetTexture.Width,
					height: TargetTexture.Height,
					mipLevels: 1,
					arrayLayers: 1,
					format: PixelFormat.B8_G8_R8_A8_UNorm,
					usage: TextureUsage.RenderTarget | TextureUsage.Sampled ) );

				DepthTexture = device.ResourceFactory.CreateTexture( TextureDescription.Texture2D(
					width: TargetTexture.Width,
					height: TargetTexture.Height,
					mipLevels: 1,
					arrayLayers: 1,
					format: PixelFormat.D32_Float_S8_UInt,
					usage: TextureUsage.DepthStencil ) );

				RenderFramebuffer = device.ResourceFactory.CreateFramebuffer( new( DepthTexture, ViewTexture ) );
				RenderSize = new( Window.Size.X, Window.Size.Y );

				WindowSet = device.ResourceFactory.CreateSet( Render.Layouts.Window, ViewTexture, Render.Samplers.LinearBorder );
			};

			regenerateBuffers( false );

			window.FramebufferResize += ( newSize ) =>
			{
				TargetSwapchain.Resize( (uint)newSize.X, (uint)newSize.Y );
				regenerateBuffers( true );
			};
		}

		public int Mask { get; set; } = int.MinValue;

		public IWindow? Window { get; private set; } = null;
		public Vector2I RenderSize { get; set; }

		/// <summary>
		/// Framebuffer associated with <see cref="TargetSwapchain"/> for displaying the view onto the window.
		/// In the case of a custom render target, it's just a framebuffer to perform post-processing on.
		/// </summary>
		public Framebuffer Framebuffer { get; private set; }

		/// <summary>
		/// The texture associated with either the <see cref="Window"/> or a custom render target.
		/// Doesn't have a depth buffer, meant to be a target of post-processing.
		/// </summary>
		public Texture? TargetTexture { get; private set; } = null;

		/// <summary>
		/// The swapchain associated with the <see cref="Window"/>. If that is null, this is null too.
		/// </summary>
		public Swapchain? TargetSwapchain { get; private set; } = null;

		/// <summary>
		/// Framebuffer associated with <see cref="ViewTexture"/> and <see cref="DepthTexture"/> for rendering.
		/// </summary>
		public Framebuffer RenderFramebuffer { get; private set; }

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

		public Matrix4x4 Transform
		{
			get => mCameraData.Transform;
			set
			{
				mCameraData.Transform = value;
				TransformOrProjectionChanged = true;
			}
		}

		public Matrix4x4 Projection
		{
			get => mCameraData.Projection;
			set
			{
				mCameraData.Projection = value;
				TransformOrProjectionChanged = true;
			}
		}

		public bool TransformOrProjectionChanged { get; set; } = false;
		public DeviceBuffer CameraBuffer { get; private set; }
		public ResourceSet PerViewSet { get; private set; }

		internal void UpdateBuffers( GraphicsDevice device )
		{
			if ( TransformOrProjectionChanged )
			{
				device.UpdateBuffer( CameraBuffer, 0, mCameraData );
			}
		}

		public void Dispose()
		{
			PerViewSet.Dispose();
			WindowSet.Dispose();
			RenderFramebuffer.Dispose();

			ViewTexture.Dispose();
			DepthTexture.Dispose();
			CameraBuffer.Dispose();
		}
	}
}
