using Avalonia.Skia;
using SkiaSharp;
using TerraFX.Interop.Vulkan;
using Veldrid;

namespace Elegy.Avalonia.Embedded;

/// <summary>Encapsulates a Skia surface along with the Elegy texture it comes from.</summary>
internal sealed class ElegySkiaSurface : ISkiaSurface
{
	public SKSurface SkSurface { get; }

	public Texture GdTexture { get; }

	public VkImage VkImage { get; }

	public GraphicsDevice RenderingDevice { get; }

	public double RenderScaling { get; set; }

	public VkImageLayout LastLayout { get; set; }

	public ulong DrawCount { get; set; }

	public bool IsDisposed { get; private set; }

	public void TransitionLayoutTo( VkImageLayout newLayout )
	{
		if ( LastLayout == newLayout )
			return;

		var sourceAccessMask = LastLayout switch
		{
			VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL => VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_READ_BIT,
			VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL => VkAccessFlags.VK_ACCESS_SHADER_READ_BIT,
			_                                      => VkAccessFlags.VK_ACCESS_MEMORY_READ_BIT | VkAccessFlags.VK_ACCESS_MEMORY_WRITE_BIT
		};

		var destinationAccessMask = newLayout switch
		{
			VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL => VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
			VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL => VkAccessFlags.VK_ACCESS_SHADER_WRITE_BIT,
			_                                      => VkAccessFlags.VK_ACCESS_MEMORY_READ_BIT | VkAccessFlags.VK_ACCESS_MEMORY_WRITE_BIT
		};

		// TODO: Elegy-Avalonia: transition image layout
		//BarrierHelper.TransitionImageLayout( VkImage, LastLayout, sourceAccessMask, newLayout, destinationAccessMask );
		RenderingDevice.GetVulkanInfo().TransitionImageLayout( GdTexture, (uint)newLayout );
		LastLayout = newLayout;
	}

	SKSurface ISkiaSurface.Surface
		=> SkSurface;

	bool ISkiaSurface.CanBlit
		=> false;

	public ElegySkiaSurface(
		SKSurface skSurface,
		Texture gdTexture,
		VkImage vkImage,
		VkImageLayout lastLayout,
		GraphicsDevice renderingDevice,
		double renderScaling
	)
	{
		SkSurface = skSurface;
		GdTexture = gdTexture;
		VkImage = vkImage;
		LastLayout = lastLayout;
		RenderingDevice = renderingDevice;
		RenderScaling = renderScaling;
		IsDisposed = false;
	}

	void ISkiaSurface.Blit( SKCanvas canvas )
		=> throw new NotSupportedException();

	public void Dispose()
	{
		if ( IsDisposed )
			return;

		IsDisposed = true;
		SkSurface.Dispose();
		GdTexture.Dispose();
		RenderingDevice.Dispose();
	}
}
