// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Elegy.Common.Assets;
using ElegyView = Elegy.RenderSystem.Objects.View;
using ElegyTexture = Elegy.AssetSystem.Resources.Texture;
using ElegyRender = Elegy.RenderSystem.API.Render;

namespace Elegy.Avalonia.Platform;

// TODO: Elegy.Avalonia: implement Elegy viewport control

/// <summary>
/// Elegy renderable viewport for Avalonia UI.
/// </summary>
/// <remarks>
/// Assumes that it's a descendant of <see cref="ElegyWindow"/>.
/// </remarks>
public partial class ElegyViewport : Control
{
	private CompositionSurfaceVisual? mVisual;
	private Compositor? mCompositor;
	private readonly Action mUpdate;
	private bool mUpdateQueued;
	private bool mInitialized;
	private ElegyView? mEngineView;

	public CompositionDrawingSurface? Surface { get; protected set; }
	public ICompositionGpuInterop? GpuInterop { get; protected set; }
	public ElegyWindow ParentWindow { get; }
	public AvaloniaInputContext InputContext => ParentWindow.InputContext;
	public ElegyView? EngineView { get => mEngineView; protected set => mEngineView = value; }
	public string ErrorMessage { get; protected set; } = string.Empty;

	public ElegyViewport()
	{
		ParentWindow = this.FindAncestorOfType<ElegyWindow>()!;
		mUpdate = UpdateFrame;
	}

	protected override void OnAttachedToVisualTree( VisualTreeAttachmentEventArgs e )
	{
		base.OnAttachedToVisualTree( e );
		Initialize();
	}

	protected override void OnDetachedFromLogicalTree( LogicalTreeAttachmentEventArgs e )
	{
		if ( mInitialized )
		{
			FreeGraphicsResources();
		}

		mInitialized = false;
		base.OnDetachedFromLogicalTree( e );
	}

	private async void Initialize()
	{
		try
		{
			var selfVisual = ElementComposition.GetElementVisual( this )!;
			mCompositor = selfVisual.Compositor;

			Surface = mCompositor.CreateDrawingSurface();
			mVisual = mCompositor.CreateSurfaceVisual();
			mVisual.Size = new( Bounds.Width, Bounds.Height );
			mVisual.Surface = Surface;
			ElementComposition.SetElementChildVisual( this, mVisual );

			(bool res, string info) = await DoInitialize( mCompositor, Surface );
			ErrorMessage = info;
			mInitialized = res;
			QueueNextFrame();
		}
		catch ( Exception e )
		{
			ErrorMessage = e.ToString();
		}
	}

	private void UpdateFrame()
	{
		mUpdateQueued = false;
		var root = this.GetVisualRoot();
		if ( root == null )
			return;

		mVisual!.Size = new( Bounds.Width, Bounds.Height );
		PixelSize size = PixelSize.FromSize( Bounds.Size, root.RenderScaling );
		RenderFrame( size );
		QueueNextFrame();
	}

	private void QueueNextFrame()
	{
		if ( mInitialized && !mUpdateQueued && mCompositor != null )
		{
			mUpdateQueued = true;
			mCompositor?.RequestCompositionUpdate( mUpdate );
		}
	}

	protected override void OnPropertyChanged( AvaloniaPropertyChangedEventArgs change )
	{
		if ( change.Property == BoundsProperty )
		{
			QueueNextFrame();
		}

		base.OnPropertyChanged( change );
	}

	private async Task<(bool success, string info)> DoInitialize(
		Compositor compositor,
		CompositionDrawingSurface compositionDrawingSurface )
	{
		GpuInterop = await compositor.TryGetCompositionGpuInterop();
		if ( GpuInterop == null )
		{
			return (false, "Compositor doesn't support interop for the current backend");
		}

		return InitializeGraphicsResources( compositor, compositionDrawingSurface, GpuInterop );
	}

	protected (bool success, string info) InitializeGraphicsResources(
		Compositor compositor,
		CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop gpuInterop )
	{
		// TODO: implement a faux swapchain using a View with a few circling framebuffers
		TextureMetadata textureInfo = new()
		{
			Width = (uint)Bounds.Width,
			Height = (uint)Bounds.Height
		};

		var renderTexture = ElegyRender.CreateTexture( textureInfo );
		EngineView = ElegyRender.CreateView( renderTexture );

		return (true, "OK");
	}

	protected void FreeGraphicsResources()
	{
		ElegyRender.FreeView( ref mEngineView );
	}

	protected void RenderFrame( PixelSize pixelSize )
	{
		if ( EngineView is null || Surface is null )
		{
			return;
		}

		// TODO: Refer to SwapchainBase for all this stuff perhaps
		// https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Base/Rendering/SwapchainBase.cs
		ElegyRender.RenderFrame( EngineView );

		// TODO: Export Vulkan semaphores
		// https://github.com/AvaloniaUI/Avalonia/blob/master/samples/GpuInterop/VulkanDemo/VulkanSemaphorePair.cs#L69-L75
		ICompositionImportedGpuSemaphore renderCompletedSemaphore; //GpuInterop.ImportSemaphore( ...Export() );
		ICompositionImportedGpuSemaphore availableSemaphore; //GpuInterop.ImportSemaphore( ...Export() );
		// Notes: this interop.Import( object.Export() ) pattern can really just be:
		// ConvertToAvaloniaCompositionObject( object );

		// TODO: Export the Vulkan image:
		// https://github.com/AvaloniaUI/Avalonia/blob/master/samples/GpuInterop/VulkanDemo/VulkanImage.cs#L213-L231
		ICompositionImportedGpuImage importedImage; //GpuInterop.ImportImage( CurrentFrameTexture.Export() );
		// Notes: lastPresent is basically a Task. It can tell us whether the image is ready or broken.
		// One semaphore (availableSem.) is used to transition the image layout *when* the image is available,
		// when beginning to draw. The other (renderCompletedSem.) is used when presenting, also to transition
		// image layouts.
		// Basically, UpdateWithSemaphoresAsync will read the image with respect to the semaphores. It'll wait
		// until stuff is done rendering and will signal to us when the image is available.

		// TODO: Wrap Veldrid Vulkan images, semaphores etc. for import/export:
		// https://github.com/AvaloniaUI/Avalonia/blob/master/samples/GpuInterop/VulkanDemo/VulkanSwapchain.cs#L107-L145
		throw new NotImplementedException( "Elegy-Avalonia rendering is not implemented" );
		Task lastPresent = Surface.UpdateWithSemaphoresAsync( importedImage, renderCompletedSemaphore, availableSemaphore );
	}
}
