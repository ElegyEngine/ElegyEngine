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

	public CompositionDrawingSurface? Surface { get; protected set; }
	public ElegyWindow ParentWindow { get; }
	public AvaloniaInputContext InputContext => ParentWindow.InputContext;
	public ElegyView? EngineView { get; protected set; }
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

	async void Initialize()
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

			var (res, info) = await DoInitialize( mCompositor, Surface );
			ErrorMessage = info;
			mInitialized = res;
			QueueNextFrame();
		}
		catch ( Exception e )
		{
			ErrorMessage = e.ToString();
		}
	}

	void UpdateFrame()
	{
		mUpdateQueued = false;
		var root = this.GetVisualRoot();
		if ( root == null )
			return;

		mVisual!.Size = new( Bounds.Width, Bounds.Height );
		var size = PixelSize.FromSize( Bounds.Size, root.RenderScaling );
		RenderFrame( size );
		QueueNextFrame();
	}

	void QueueNextFrame()
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

	async Task<(bool success, string info)> DoInitialize(
		Compositor compositor,
		CompositionDrawingSurface compositionDrawingSurface )
	{
		var interop = await compositor.TryGetCompositionGpuInterop();
		if ( interop == null )
		{
			return (false, "Compositor doesn't support interop for the current backend");
		}

		return InitializeGraphicsResources( compositor, compositionDrawingSurface, interop );
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
	}

	protected void RenderFrame( PixelSize pixelSize )
	{
		if ( EngineView is null || Surface is null )
		{
			return;
		}

		throw new NotImplementedException( "Elegy-Avalonia rendering is not implemented" );

		ElegyRender.RenderFrame( EngineView );

		ICompositionImportedGpuImage importedImage = null!;
		ICompositionImportedGpuSemaphore waitForSemaphore = null!;
		ICompositionImportedGpuSemaphore signalSemaphore = null!;
		Surface.UpdateWithSemaphoresAsync( importedImage, waitForSemaphore, signalSemaphore );
	}
}
