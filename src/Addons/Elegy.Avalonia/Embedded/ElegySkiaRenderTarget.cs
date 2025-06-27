using System.Diagnostics.CodeAnalysis;
using Avalonia.Skia;
using SkiaSharp;

namespace Elegy.Avalonia.Embedded;

/// <summary>A render target that uses an underlying Skia surface.</summary>
internal sealed class ElegySkiaRenderTarget : ISkiaGpuRenderTarget
{
	private readonly ElegySkiaSurface mSurface;
	private readonly GRContext mGrContext;
	private readonly double mRenderScaling;

	[SuppressMessage( "ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Doesn't affect correctness" )]
	public bool IsCorrupted
		=> mSurface.IsDisposed || mGrContext.IsAbandoned || mRenderScaling != mSurface.RenderScaling;

	public ElegySkiaRenderTarget( ElegySkiaSurface surface, GRContext grContext )
	{
		mRenderScaling = surface.RenderScaling;
		mSurface = surface;
		mGrContext = grContext;
	}

	public ISkiaGpuRenderSession BeginRenderingSession()
		=> new ElegySkiaGpuRenderSession( mSurface, mGrContext );

	public void Dispose()
	{
	}
}
