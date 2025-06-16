using System.Diagnostics;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.OpenXR;
using Silk.NET.OpenXR.Extensions.KHR;
using TerraFX.Interop.Vulkan;

namespace XrResearch
{
	public static partial class Program
	{
		private static KhrVulkanEnable? xrVulkan;
		private static GraphicsRequirementsVulkanKHR mVulkanGraphicsRequirements;

		public static bool GetVulkanVrExtension()
		{
			if ( !Xr.TryGetInstanceExtension( null, mInstance, out xrVulkan ) )
			{
				Console.WriteLine( "Failed to get Vulkan XR extension" );
				Debugger.Break();
				return false;
			}

			mVulkanGraphicsRequirements = new() { Type = StructureType.GraphicsRequirementsVulkanKhr };
			XrCheck( xrVulkan.GetVulkanGraphicsRequirements( mInstance, mSystemId, ref mVulkanGraphicsRequirements ) );

			var minVersion = mVulkanGraphicsRequirements.MinApiVersionSupported;
			Console.WriteLine( $"OpenXR needs Vulkan {XrVersionMajor( minVersion )}.{XrVersionMinor( minVersion )}" );
			
			return true;
		}

		public static IntPtr VulkanVrGetPhysicalDevice( IntPtr vulkanInstance, VkPhysicalDevice[] availableDevices )
		{
			VkHandle xrPhysicalDevice = default;
			XrCheck( xrVulkan.GetVulkanGraphicsDevice( mInstance, mSystemId, new( vulkanInstance ), ref xrPhysicalDevice ) );
			return xrPhysicalDevice.Handle;
		}

		public static unsafe string[] GetVulkanVrDeviceExtensions()
		{
			// TODO: Trim the null terminators at the end, it's giving Veldrid a fit
			uint characterCount = 0;
			XrCheck( xrVulkan.GetVulkanDeviceExtension( mInstance, mSystemId, 0, ref characterCount, (byte*)null ) );
			byte[] bytes = new byte[characterCount];
			XrCheck( xrVulkan.GetVulkanDeviceExtension( mInstance, mSystemId, ref characterCount, bytes.AsSpan() ) );

			return Encoding.UTF8.GetString( bytes.AsSpan().TrimEnd( (byte)'\0' ) )
				//.Replace( "VK_EXT_debug_marker", "VK_EXT_debug_utils" )
				//.Replace( "VK_EXT_debug_marker", "" )
				.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
		}

		public static unsafe string[] GetVulkanVrInstanceExtensions()
		{
			uint characterCount = 0;
			XrCheck( xrVulkan.GetVulkanInstanceExtension( mInstance, mSystemId, 0, ref characterCount, (byte*)null ) );
			byte[] bytes = new byte[characterCount];
			XrCheck( xrVulkan.GetVulkanInstanceExtension( mInstance, mSystemId, ref characterCount, bytes.AsSpan() ) );

			return Encoding.UTF8.GetString( bytes.AsSpan().TrimEnd( (byte)'\0' ) ).Split( ' ' );
		}
	}
}
