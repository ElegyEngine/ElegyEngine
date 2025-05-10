
using System.Buffers;
using System.Text.Unicode;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using Veldrid;
using Elegy.RenderSystem.API;
using TerraFX.Interop.Vulkan;
using PixelFormat = Avalonia.Remote.Protocol.Viewport.PixelFormat;

namespace Elegy.Avalonia;

/// <summary>Bridges the Elegy Vulkan renderer with a Skia context used by Avalonia.</summary>
internal sealed class ElegySkiaGpu : ISkiaGpu
{
	private readonly GraphicsDevice mDevice;
	private readonly GRContext mGrContext;
	private readonly uint mQueueFamilyIndex;

	public bool IsLost
		=> mGrContext.IsAbandoned;

	public unsafe ElegySkiaGpu()
	{
		IntPtr GetVkProcAddress( string name, IntPtr instance, IntPtr device )
		{
			Span<byte> utf8Name = stackalloc byte[128];

			// The stackalloc buffer should always be sufficient for proc names
			if ( Utf8.FromUtf16( name, utf8Name[..^1], out _, out var bytesWritten ) != OperationStatus.Done )
				throw new InvalidOperationException( $"Invalid proc name {name}" );

			utf8Name[bytesWritten] = 0;

			fixed ( byte* utf8NamePtr = utf8Name )
			{
				return IntPtr.Zero;
				//return device != IntPtr.Zero
				//	? vkGetDeviceProcAddr( new VkDevice( device ), utf8NamePtr )
				//	: vkGetInstanceProcAddr( new VkInstance( instance ), utf8NamePtr );
			}
		}

		mDevice = Render.Device;
		var vulkanInfo = mDevice.GetVulkanInfo();

		var vkContext = new GRVkBackendContext
		{
			VkInstance = vulkanInfo.Instance,
			VkPhysicalDevice = vulkanInfo.PhysicalDevice,
			VkDevice = vulkanInfo.Device,
			VkQueue = vulkanInfo.GraphicsQueue,
			GraphicsQueueIndex = vulkanInfo.GraphicsQueueFamilyIndex,
			GetProcedureAddress = GetVkProcAddress
		};

		if ( GRContext.CreateVulkan( vkContext ) is not { } grContext )
			throw new InvalidOperationException( "Couldn't create Vulkan context" );

		mGrContext = grContext;
		mQueueFamilyIndex = vulkanInfo.GraphicsQueueFamilyIndex;
	}

	object? IOptionalFeatureProvider.TryGetFeature( Type featureType )
		=> null;

	IDisposable IPlatformGraphicsContext.EnsureCurrent()
		=> EmptyDisposable.Instance;

	ISkiaGpuRenderTarget? ISkiaGpu.TryCreateRenderTarget( IEnumerable<object> surfaces )
		=> surfaces.OfType<ElegySkiaSurface>().FirstOrDefault() is { } surface
			? new ElegySkiaRenderTarget( surface, mGrContext )
			: null;

	public ElegySkiaSurface CreateSurface( PixelSize size, double renderScaling )
	{
		size = new PixelSize( Math.Max( size.Width, 1 ), Math.Max( size.Height, 1 ) );

		var elTexture = mDevice.ResourceFactory.CreateTexture( new()
		{
			Format = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
			Type = TextureType.Texture2D,
			Width = (uint)size.Width,
			Height = (uint)size.Height,
			Depth = 1,
			ArrayLayers = 1,
			MipLevels = 1,
			SampleCount = TextureSampleCount.Count1,
			Usage = TextureUsage.Sampled | TextureUsage.RenderTarget
		} );

		var vkImage = new VkImage( mDevice.GetDriverResource( RenderingDevice.DriverResource.Texture, gdRdTexture, 0UL ) );
		if ( vkImage.Value == 0UL )
			throw new InvalidOperationException( "Couldn't get Vulkan image from Elegy texture" );

		var vkFormat = (uint)mDevice.GetDriverResource( RenderingDevice.DriverResource.TextureDataFormat, gdRdTexture, 0UL );
		if ( vkFormat == 0U )
			throw new InvalidOperationException( "Couldn't get Vulkan format from Elegy texture" );

		var grVkImageInfo = new GRVkImageInfo
		{
			CurrentQueueFamily = mQueueFamilyIndex,
			Format = vkFormat,
			Image = vkImage.Value,
			ImageLayout = (uint)VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL,
			ImageTiling = (uint)VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
			ImageUsageFlags = (uint)(
				VkImageUsageFlags.VK_IMAGE_USAGE_SAMPLED_BIT      |
				VkImageUsageFlags.VK_IMAGE_USAGE_TRANSFER_SRC_BIT |
				VkImageUsageFlags.VK_IMAGE_USAGE_TRANSFER_DST_BIT |
				VkImageUsageFlags.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT
			),
			LevelCount = 1,
			SampleCount = 1,
			Protected = false,
			SharingMode = (uint)VkSharingMode.VK_SHARING_MODE_EXCLUSIVE
		};

		var skSurface = SKSurface.Create(
			mGrContext,
			new GRBackendRenderTarget( size.Width, size.Height, 1, grVkImageInfo ),
			GRSurfaceOrigin.TopLeft,
			SKColorType.Rgba8888,
			new SKSurfaceProperties( SKPixelGeometry.RgbHorizontal )
		);

		if ( skSurface is null )
			throw new InvalidOperationException( "Couldn't create Skia surface from Vulkan image" );

		//var gdTexture = new Texture2Drd
		//{
		//	TextureRdRid = gdRdTexture
		//};

		var surface = new ElegySkiaSurface(
			skSurface,
			gdTexture,
			vkImage,
			VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED,
			mDevice,
			renderScaling
		);

		surface.TransitionLayoutTo( VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );

		return surface;
	}

	ISkiaSurface? ISkiaGpu.TryCreateSurface( PixelSize size, ISkiaGpuRenderSession? session )
		=> session is ElegySkiaGpuRenderSession elegySession
			? CreateSurface( size, elegySession.Surface.RenderScaling )
			: null;

	public void Dispose()
	{
		mGrContext.Dispose();
	}
}
