// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.API;
using Elegy.Engine.Interfaces;
using Elegy.Engine.Interfaces.Rendering;

using Elegy.RenderBackend;
using Elegy.RenderBackend.Extensions;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderStandard;

public partial class RenderStandard : IRenderFrontend
{
	private RenderPipeline mTrianglePipeline;
	private ResourceLayout mTriangleLayout;
	
	// Render view+depth into a window view framebufer
	private RenderPipeline mWindowPipeline;
	private ResourceLayout mWindowLayout;
	private Sampler mWindowSampler;

	[StructLayout(LayoutKind.Sequential)]
	private struct WindowVertex
	{
		public Vector2 Position { get; set; }
		public Vector2 Uv { get; set; }
	}

	private void InitialiseCorePipelines()
	{
		mWindowSampler = Factory.CreateSampler( SamplerDescription.Linear );

		mWindowLayout = Factory.CreateLayout(
			new ResourceLayoutElementDescription( "ViewTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment ),
			new ResourceLayoutElementDescription( "ViewSampler", ResourceKind.Sampler, ShaderStages.Fragment ) );

		mWindowPipeline = Factory.CreateGraphicsPipeline<WindowVertex>(
			shaderPath: GetShaderPath( "shaders/bin/Window.DEFAULT" ),
			preset: RasterPreset.NoDepthTwoSided,
			outputDescription: new OutputDescription()
			{
				SampleCount = TextureSampleCount.Count1,
				DepthAttachment = null,
				ColorAttachments =
				[
					new( PixelFormat.B8_G8_R8_A8_UNorm )
				],
			}, resourceLayouts: mWindowLayout );

		mTrianglePipeline = Factory.CreateGraphicsPipeline<WindowVertex>(
			shaderPath: GetShaderPath( "shaders/bin/Debug.DEFAULT" ),
			preset: RasterPreset.NoDepthTwoSided,
			outputDescription: new OutputDescription()
			{
				SampleCount = TextureSampleCount.Count1,
				DepthAttachment = new( PixelFormat.D32_Float_S8_UInt ),
				ColorAttachments =
				[
					new( PixelFormat.B8_G8_R8_A8_UNorm )
				],
			});

		InitialiseDebugMeshes();
	}

	private static readonly List<WindowVertex> FullquadVertices = new()
	{
		new() { Position = new( -1.0f, -1.0f ), Uv = new( 0.0f, 0.0f ) },
		new() { Position = new( -1.0f, 1.0f ), Uv = new( 0.0f, 1.0f ) },
		new() { Position = new( 1.0f, 1.0f ), Uv = new( 1.0f, 1.0f ) },
		new() { Position = new( 1.0f, -1.0f ), Uv = new( 1.0f, 0.0f ) }
	};

	private static readonly List<uint> FullquadIndices = new()
	{
		0, 1, 2,
		0, 2, 3
	};

	private static readonly List<WindowVertex> TriangleVertices = new()
	{
		new() { Position = new( -0.5f, -0.5f ), Uv = new( 0.0f, 0.0f ) },
			new() { Position = new( 0.0f, 0.5f ), Uv = new( 0.5f, 1.0f ) },
			new() { Position = new( 0.5f, -0.5f ), Uv = new( 1.0f, 0.0f ) }
	};

	private static readonly List<uint> TriangleIndices = new()
	{
		0, 1, 2
	};

	private DeviceBuffer mFullquadVertexBuffer;
	private DeviceBuffer mFullquadIndexBuffer;
	private DeviceBuffer mTriangleVertexBuffer;
	private DeviceBuffer mTriangleIndexBuffer;

	private void InitialiseDebugMeshes()
	{
		mFullquadVertexBuffer = Device.CreateBufferFromList( BufferUsage.VertexBuffer, FullquadVertices );
		mFullquadIndexBuffer = Device.CreateBufferFromList( BufferUsage.IndexBuffer, FullquadIndices );

		mTriangleVertexBuffer = Device.CreateBufferFromList( BufferUsage.VertexBuffer, TriangleVertices );
		mTriangleIndexBuffer = Device.CreateBufferFromList( BufferUsage.IndexBuffer, TriangleIndices );
	}

	// This is a hack! Find a way to do this more properly later on
	private static string GetShaderPath( string shaderPath, bool compute = false )
	{
		string? path = FileSystem.PathTo( $"{shaderPath}.{(compute ? "cs" : "ps")}.spv", PathFlags.File );

		if ( path is null )
		{
			Console.Error( $"Can't find shader '{shaderPath}'" );
			return shaderPath;
		}

		return Path.ChangeExtension( Path.ChangeExtension( path, null ), null );
	}

	double mCpuTime = 0.0;
	double mGpuTime = 0.0;
	double mPresentTime = 0.0;

	public void BeginFrame()
	{
		mCpuTime = Core.Seconds;
	}

	public void EndFrame()
	{
		mCpuTime = Core.Seconds - mCpuTime;
		mGpuTime = Core.Seconds;

		mDevice.WaitForIdle();

		mGpuTime = Core.Seconds - mGpuTime;
	}

	public void RenderView( in IView view )
	{
		RenderView? rview = view as RenderView;
		Debug.Assert( rview is not null );

		mRenderCommands.Begin();

		// Render a triangle into a framebuffer
		mRenderCommands.SetFramebuffer( rview.RenderFramebuffer );
		mRenderCommands.ClearColorTarget( 0, new( 0.01f, 0.05f, 0.06f, 1.0f ) );
		mRenderCommands.ClearDepthStencil( 0.0f );
		mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, rview.RenderSize.X, rview.RenderSize.Y, 0.0f, 1.0f ) );

		mRenderCommands.SetPipeline( mTrianglePipeline.Pipeline );
		mRenderCommands.SetVertexBuffer( 0, mTriangleVertexBuffer );
		mRenderCommands.SetIndexBuffer( mTriangleIndexBuffer, IndexFormat.UInt32 );

		mRenderCommands.DrawIndexed( 3 );

		// A triangle has been drawn into a framebuffer
		// Now render that framebuffer onto the window
		mRenderCommands.SetFramebuffer( rview.Framebuffer );
		mRenderCommands.ClearColorTarget( 0, new( 0.02f, 0.10f, 0.12f, 1.0f ) );
		mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, rview.RenderSize.X, rview.RenderSize.Y, 0.0f, 1.0f ) );
		
		mRenderCommands.SetPipeline( mWindowPipeline.Pipeline );
		mRenderCommands.SetGraphicsResourceSet( 0, rview.WindowSet );
		mRenderCommands.SetVertexBuffer( 0, mFullquadVertexBuffer );
		mRenderCommands.SetIndexBuffer( mFullquadIndexBuffer, IndexFormat.UInt32 );

		mRenderCommands.DrawIndexed( 6 );

		mRenderCommands.End();
		Device.SubmitCommands( mRenderCommands );
	}

	public void PresentView( in IView view )
	{
		Debug.Assert( view.TargetSwapchain is not null );

		mPresentTime = Core.Seconds;
		mDevice.SwapBuffers( view.TargetSwapchain );
		mPresentTime = Core.Seconds - mPresentTime;

		//mLogger.Log( "New frame" );
		//mLogger.Log( $"CPU: {mCpuTime * 1000.0 * 1000.0} us" );
		//mLogger.Log( $"GPU: {mGpuTime * 1000.0 * 1000.0} us" );
		//mLogger.Log( $"SWP: {mPresentTime * 1000.0 * 1000.0} us" );
	}
}
