// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.FileSystem.API;
using Elegy.RenderBackend;
using Elegy.RenderBackend.Extensions;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Interfaces.Rendering;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Elegy.RenderStandard;

public partial class RenderStandard : IRenderFrontend
{
	// Render view+depth into a window view framebufer
	private RenderPipeline mWindowPipeline;
	private ResourceLayout mWindowLayout;
	private Sampler mWindowSampler;

	private static readonly int[] HardcodedLayoutIds = [ 0, 1 ];
	private ResourceLayout mPerViewLayout;
	private ResourceLayout mPerEntityLayout;

	internal static Sampler NearestSampler { get; private set; }
	internal static Sampler LinearSampler { get; private set; }

	private static readonly OutputDescription OutputDescNormal = new()
	{
		SampleCount = TextureSampleCount.Count1,
		DepthAttachment = new( PixelFormat.D32_Float_S8_UInt ),
		ColorAttachments =
		[
			new( PixelFormat.B8_G8_R8_A8_UNorm )
		]
	};

	private static readonly OutputDescription OutputDescDepthOnly = new()
	{
		SampleCount = TextureSampleCount.Count1,
		DepthAttachment = new( PixelFormat.D32_Float_S8_UInt ),
		ColorAttachments = null
	};

	private static readonly OutputDescription OutputDescBackbuffer = new()
	{
		SampleCount = TextureSampleCount.Count1,
		DepthAttachment = null,
		ColorAttachments =
		[
			new( PixelFormat.B8_G8_R8_A8_UNorm )
		]
	};

	[StructLayout(LayoutKind.Sequential)]
	private struct WindowVertex
	{
		public Vector2 Position { get; set; }
		public Vector2 Uv { get; set; }
	}

	private static OutputDescription GetOutputForShaderVariant( RenderBackend.Assets.ShaderTemplateEntry entry, bool postprocessHint )
	{
		if ( entry.ShaderDefine.ToLower().Contains( "depth" ) )
		{
			return OutputDescDepthOnly;
		}

		return postprocessHint ? OutputDescBackbuffer : OutputDescNormal;
	}

	private static string? FindShaderBinaryPath( string path )
	{
		string? result = Files.PathTo( $"{path}.ps.spv", PathFlags.File );
		if ( result is not null )
		{
			return result.Replace( ".ps.spv", null );
		}

		result = Files.PathTo( $"{path}.cs.spv", PathFlags.File );
		if ( result is not null )
		{
			return result.Replace( ".cs.spv", null );
		}

		return null;
	}

	public bool CreateCorePipelines()
	{
		bool allOkay = true;
		foreach ( var template in Render.MaterialTemplates )
		{
			if ( !template.CompileResources( mDevice, GetOutputForShaderVariant, HardcodedLayoutIds, FindShaderBinaryPath ) )
			{
				mLogger.Error( $"Failed to compile pipeline for material template '{template.Data.Name}'" );
				allOkay = false;
			}
		}

		NearestSampler = Factory.CreateSampler( SamplerDescription.Point );
		LinearSampler = Factory.CreateSampler( SamplerDescription.Linear );

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

		mPerEntityLayout = Factory.CreateLayout(
			new ResourceLayoutElementDescription( "uView", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment ) );

		mPerViewLayout = Factory.CreateLayout( 
			new ResourceLayoutElementDescription( "uEntity", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment ) );

		InitialiseDebugMeshes();
		return allOkay;
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

	private DeviceBuffer mFullquadVertexBuffer;
	private DeviceBuffer mFullquadIndexBuffer;

	private void InitialiseDebugMeshes()
	{
		mFullquadVertexBuffer = Device.CreateBufferFromList( BufferUsage.VertexBuffer, FullquadVertices );
		mFullquadIndexBuffer = Device.CreateBufferFromList( BufferUsage.IndexBuffer, FullquadIndices );
	}

	// This is a hack! Find a way to do this more properly later on
	private static string GetShaderPath( string shaderPath, bool compute = false )
	{
		string? path = Files.PathTo( $"{shaderPath}.{(compute ? "cs" : "ps")}.spv", PathFlags.File );

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

	Stopwatch mStopwatch = new();

	private double GetSeconds() => (double)mStopwatch.ElapsedTicks / Stopwatch.Frequency;

	public void BeginFrame()
	{
		mCpuTime = GetSeconds();
	}

	public void EndFrame()
	{
		mCpuTime = GetSeconds() - mCpuTime;
		mGpuTime = GetSeconds();

		mDevice.WaitForIdle();

		mGpuTime = GetSeconds() - mGpuTime;
	}

	public void RenderSingleEntity( RenderEntity entity, RenderView view )
	{
		RenderMesh mesh = (RenderMesh)entity.Mesh;
		for ( int i = 0; i < mesh.Submeshes.Count; i++ )
		{
			// TODO: sort render ents by material
			var submesh = mesh.Submeshes[i];
			var submaterial = mesh.Materials[i];
			// TODO: I don't want to query a dictionary at runtime for this,
			// figure out a faster way to access the stuff
			var shaderVariant = submaterial.Template.ShaderVariants["GENERAL"];

			mRenderCommands.SetPipeline( shaderVariant.Pipeline );

			// We have a few hardcoded resource set IDs
			// 0 is always per-frame/per-view data (all about the camera basically)
			// 1 is always per-entity data (entity transform matrix, bone matrices etc.)
			mRenderCommands.SetGraphicsResourceSet( 0, view.PerViewSet );
			mRenderCommands.SetGraphicsResourceSet( 1, entity.PerEntitySet );

			// Set shader parametres used by this shader variant
			// E.g. variant A might not use certain buffers so it omits an entire resource set,
			// while variant B might have them in a different order. Anything can happen after
			// the first few hardcoded sets 0 and 1
			var shaderVariantSets = submaterial.ResourceSets["GENERAL"];
			for ( int resourceSetId = 0; resourceSetId < shaderVariantSets.Length; resourceSetId++ )
			{
				mRenderCommands.SetGraphicsResourceSet(
					// Maintain correct slot
					(uint)shaderVariant.ResourceMappings[resourceSetId].SetId,
					// Each shader variant provides its own copy of resource sets
					// It's a bit wasteful but was simpler to implement
					shaderVariantSets[resourceSetId] );
			}

			// Send vertex buffers used by this shader variant
			// I wonder if this would get any faster using 'unsafe' or Span<T>, but
			// frankly this is not yet at a stage where it's worth profiling much
			for ( int vertexAttributeId = 0; vertexAttributeId < shaderVariant.VertexAttributes.Length; vertexAttributeId++ )
			{
				var vertexAttribute = shaderVariant.VertexAttributes[vertexAttributeId];
				var buffer = submesh.GetBuffer( vertexAttribute.Semantic, vertexAttribute.Channel );

				Debug.Assert( buffer is not null, $"The '{vertexAttribute.Semantic}' buffer is MISSING" );

				mRenderCommands.SetVertexBuffer(
					(uint)vertexAttributeId,
					buffer );
			}

			// AT LAST, render the damn thing
			mRenderCommands.SetIndexBuffer( submesh.IndexBuffer, IndexFormat.UInt32 );
			mRenderCommands.DrawIndexed( submesh.NumIndices );
		}
	}

	public void SetRenderView( in RenderView view )
	{
		view.UpdateBuffers( mDevice );

		mRenderCommands.SetFramebuffer( view.RenderFramebuffer );
		mRenderCommands.ClearColorTarget( 0, new( 0.01f, 0.05f, 0.06f, 1.0f ) );
		mRenderCommands.ClearDepthStencil( 1.0f );
		mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, view.RenderSize.X, view.RenderSize.Y, 0.0f, 1.0f ) );
	}

	public void RenderViewIntoBackbuffer( in RenderView view )
	{
		mRenderCommands.SetFramebuffer( view.Framebuffer );
		mRenderCommands.ClearColorTarget( 0, new( 0.02f, 0.10f, 0.12f, 1.0f ) );
		mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, view.RenderSize.X, view.RenderSize.Y, 0.0f, 1.0f ) );

		mRenderCommands.SetPipeline( mWindowPipeline.Pipeline );
		mRenderCommands.SetGraphicsResourceSet( 0, view.WindowSet );
		mRenderCommands.SetVertexBuffer( 0, mFullquadVertexBuffer );
		mRenderCommands.SetIndexBuffer( mFullquadIndexBuffer, IndexFormat.UInt32 );

		mRenderCommands.DrawIndexed( 6 );
	}

	public void RenderView( in IView view )
	{
		Debug.Assert( view is RenderView );
		RenderView rview = (RenderView)view;

		mRenderCommands.Begin();

		// Render the viewed scene into a framebuffer
		SetRenderView( rview );

		// TODO: a lot of stuff
		foreach ( var renderEntity in mEntitySet )
		{
			RenderSingleEntity( renderEntity, rview );
		}

		// Stuff has been drawn into a framebuffer
		// Now render that framebuffer onto the window
		RenderViewIntoBackbuffer( rview );

		mRenderCommands.End();
		Device.SubmitCommands( mRenderCommands );
	}

	public void PresentView( in IView view )
	{
		Debug.Assert( view.TargetSwapchain is not null );

		mPresentTime = GetSeconds();
		mDevice.SwapBuffers( view.TargetSwapchain );
		mPresentTime = GetSeconds() - mPresentTime;

		//mLogger.Log( "New frame" );
		//mLogger.Log( $"CPU: {mCpuTime * 1000.0 * 1000.0} us" );
		//mLogger.Log( $"GPU: {mGpuTime * 1000.0 * 1000.0} us" );
		//mLogger.Log( $"SWP: {mPresentTime * 1000.0 * 1000.0} us" );
	}
}
