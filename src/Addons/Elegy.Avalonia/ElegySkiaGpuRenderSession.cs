using Avalonia.Skia;
using SkiaSharp;
using TerraFX.Interop.Vulkan;

namespace Elegy.Avalonia;

/// <summary>A render session that uses an underlying Skia surface.</summary>
internal sealed class ElegySkiaGpuRenderSession : ISkiaGpuRenderSession
{
	public ElegySkiaSurface Surface { get; }

	public GRContext GrContext { get; }

	SKSurface ISkiaGpuRenderSession.SkSurface
		=> Surface.SkSurface;

	double ISkiaGpuRenderSession.ScaleFactor
		=> Surface.RenderScaling;

	GRSurfaceOrigin ISkiaGpuRenderSession.SurfaceOrigin
		=> GRSurfaceOrigin.TopLeft;

	public ElegySkiaGpuRenderSession( ElegySkiaSurface surface, GRContext grContext )
	{
		Surface = surface;
		GrContext = grContext;

		// Clear the texture on first draw. This is already done by Avalonia, but Elegy doesn't know that.
		// We need it to avoid texture corruption on first draw on AMD GPUs. It will result in a few transparent frames after resizing.
		// TODO: find a better solution.
		//if ( Surface.DrawCount == 0 )
		//{
		//	Surface.RenderingDevice.TextureClear( Surface.GdTexture.TextureRdRid, new Color( 0u ), 0, 1, 0, 1 );
		//}

		// Elegy leaves the image in SHADER_READ_ONLY_OPTIMAL but Skia expects it in COLOR_ATTACHMENT_OPTIMAL
		Surface.TransitionLayoutTo( VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	}

	public void Dispose()
	{
		Surface.SkSurface.Flush( true );

		// Switch back to SHADER_READ_ONLY_OPTIMAL for Elegy
		Surface.TransitionLayoutTo( VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL );

		Surface.DrawCount++;
	}
}
