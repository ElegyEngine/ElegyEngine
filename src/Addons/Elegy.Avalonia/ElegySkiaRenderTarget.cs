using System.Diagnostics.CodeAnalysis;
using Avalonia.Skia;
using SkiaSharp;

namespace Elegy.Avalonia;

/// <summary>A render target that uses an underlying Skia surface.</summary>
internal sealed class ElegySkiaRenderTarget : ISkiaGpuRenderTarget
{
	private readonly ElegySkiaSurface _surface;
	private readonly GRContext _grContext;
	private readonly double _renderScaling;

	[SuppressMessage( "ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Doesn't affect correctness" )]
	public bool IsCorrupted
		=> _surface.IsDisposed || _grContext.IsAbandoned || _renderScaling != _surface.RenderScaling;

	public ElegySkiaRenderTarget( ElegySkiaSurface surface, GRContext grContext )
	{
		_renderScaling = surface.RenderScaling;
		_surface = surface;
		_grContext = grContext;
	}

	public ISkiaGpuRenderSession BeginRenderingSession()
		=> new ElegySkiaGpuRenderSession( _surface, _grContext );

	public void Dispose()
	{
	}
}
