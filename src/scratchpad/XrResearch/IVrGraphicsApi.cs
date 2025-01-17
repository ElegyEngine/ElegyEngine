using Silk.NET.OpenXR;

namespace XrResearch;

public struct SwapchainInfo
{
	public Swapchain Swapchain;
	public long SwapchainFormat;
	public List<object> ImageViews;
}

public struct ImageViewCreateInfo
{
	public object Image;
	public long Format;
	public bool IsDepth;
	public uint LayerIndex;
	public int LayerCount;
}

public interface IVrGraphicsApi
{
	long SelectColorSwapchainFormat( ReadOnlySpan<long> formats );
	long SelectDepthSwapchainFormat( ReadOnlySpan<long> formats );

	void CreateFramebuffers( in SwapchainInfo colorInfos, in SwapchainInfo depthInfos );
	object GetFramebuffer( Swapchain swapchain, uint arrayLayer );
	void DestroyFramebuffers();
	
	GraphicsBinding GetGraphicsBinding();
	object CreateSwapchainImage( Swapchain swapchain, uint index, bool depth );
	
	object CreateImageView( ImageViewCreateInfo info );
	void DestroyImageView( object imageView );

	Span<SwapchainImageBaseHeader> AllocateSwapchainImageData( Swapchain swapchain, uint count, bool depth );
	void FreeSwapchainImageData( Swapchain swapchain );
}
