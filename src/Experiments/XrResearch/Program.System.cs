using System.Diagnostics;
using Silk.NET.OpenXR;
using Silk.NET.OpenXR.Extensions.EXT;

namespace XrResearch
{
	public static partial class Program
	{
		private static ExtDebugUtils? mDebugUtilApi;

		private static readonly List<FixedUtf8String> mActiveApiLayers = new();
		private static readonly List<string> mApiLayers = new();
		private static readonly List<FixedUtf8String> mActiveInstanceExtensions = new();
		private static readonly List<string> mInstanceExtensions = new();

		private static ulong mSystemId;
		private static Instance mInstance;
		private static DebugUtilsMessengerEXT mDebugMessenger;

		public static unsafe bool CreateInstance()
		{
			Console.WriteLine( "CreateInstance" );

			ApplicationInfo xrInfo = new()
			{
				// Silk.NET 2.22 is on OpenXR 1.1.42
				ApiVersion = XrMakeVersion( 1, 0, 34 ),
				EngineVersion = 1,
				ApplicationVersion = 1
			};

			{
				ulong apiv = xrInfo.ApiVersion;
				Console.WriteLine(
					$"Requesting XR runtime version: {XrVersionMajor( apiv )}.{XrVersionMinor( apiv )}.{XrVersionPatch( apiv )}" );
			}

			"XrResearch".CStringCopyTo( xrInfo.ApplicationName, XR.MaxApplicationNameSize );
			"Elegy Engine".CStringCopyTo( xrInfo.EngineName, XR.MaxEngineNameSize );

			AddGraphicsExtensions();
			AddInputExtensions();
			mInstanceExtensions.Add( ExtDebugUtils.ExtensionName );
			mInstanceExtensions.Add( "XR_EXT_local_floor" );

			// Query API layers
			uint numApiLayers = 0;
			XrCheck( Xr.EnumerateApiLayerProperties( 0U, &numApiLayers, null ) );
			ApiLayerProperties[] apiLayers = new ApiLayerProperties[numApiLayers];
			Array.Fill( apiLayers, new() { Type = StructureType.ApiLayerProperties } );
			XrCheck( Xr.EnumerateApiLayerProperties( ref numApiLayers, apiLayers.AsSpan() ) );
			foreach ( var layerProperty in apiLayers )
			{
				string layerName = XrUnsafeExtensions.CStringToString( layerProperty.LayerName );
				Console.WriteLine( $"Layer: {layerName}" );
			}

			foreach ( var requestLayer in mApiLayers )
			{
				foreach ( var layerProperty in apiLayers )
				{
					if ( requestLayer.CStringEquals( layerProperty.LayerName ) )
					{
						mActiveApiLayers.Add( requestLayer );
						break;
					}
				}
			}

			// Query instance extensions
			uint numInstanceExtensions = 0;
			XrCheck( Xr.EnumerateInstanceExtensionProperties( (byte*)null, 0U, ref numInstanceExtensions, null ) );
			ExtensionProperties[] extensionProperties = new ExtensionProperties[numInstanceExtensions];
			Array.Fill( extensionProperties, new() { Type = StructureType.ExtensionProperties } );
			XrCheck( Xr.EnumerateInstanceExtensionProperties( (byte*)null, ref numInstanceExtensions, extensionProperties.AsSpan() ) );
			foreach ( var extension in extensionProperties )
			{
				string extensionName = XrUnsafeExtensions.CStringToString( extension.ExtensionName );
				Console.WriteLine( $"Extension: {extensionName}" );
			}

			foreach ( var requestedExtension in mInstanceExtensions )
			{
				bool found = false;

				foreach ( var extension in extensionProperties )
				{
					if ( requestedExtension.CStringEquals( extension.ExtensionName ) )
					{
						mActiveInstanceExtensions.Add( requestedExtension );
						found = true;
						break;
					}
				}

				if ( !found )
				{
					Console.WriteLine( $"Unsupported extension '{requestedExtension}'!" );
				}
			}

			// Some memory shenanigans to conform to the C API
			List<IntPtr> activeApiLayersPointers = new( mActiveApiLayers.Count );
			foreach ( var apiLayer in mActiveApiLayers )
			{
				activeApiLayersPointers.Add( apiLayer );
			}

			List<IntPtr> activeInstanceExtensionsPointers = new( mActiveInstanceExtensions.Count );
			foreach ( var instanceExtension in mActiveInstanceExtensions )
			{
				activeInstanceExtensionsPointers.Add( instanceExtension );
			}

			// Create an XR instance
			InstanceCreateInfo xrInstanceCreateInfo = new()
			{
				Type = StructureType.InstanceCreateInfo,
				CreateFlags = InstanceCreateFlags.None,
				ApplicationInfo = xrInfo,
				EnabledApiLayerCount = (uint)mActiveApiLayers.Count,
				EnabledApiLayerNames = activeApiLayersPointers.DerefDouble<byte>(),
				EnabledExtensionCount = (uint)mActiveInstanceExtensions.Count,
				EnabledExtensionNames = activeInstanceExtensionsPointers.DerefDouble<byte>()
			};

			return XrCheck( Xr.CreateInstance( xrInstanceCreateInfo, ref mInstance ), "Instance creation failed" );
		}

		public static unsafe void LogInstanceInfo()
		{
			Console.WriteLine( "LogInstanceInfo" );

			// Log some info about the OpenXR runtime
			InstanceProperties currentInstanceProperties = new() { Type = StructureType.InstanceProperties };
			XrCheck( Xr.GetInstanceProperties( mInstance, ref currentInstanceProperties ) );
			{
				ulong apiv = currentInstanceProperties.RuntimeVersion;
				string runtime = XrUnsafeExtensions.CStringToString( currentInstanceProperties.RuntimeName );

				Console.WriteLine( $"OpenXR runtime: '{runtime}'" );
				Console.WriteLine( $"OpenXR runtime version: {XrVersionMajor( apiv )}.{XrVersionMinor( apiv )}.{XrVersionPatch( apiv )}" );
			}
		}

		private static unsafe uint XrDebugLog(
			DebugUtilsMessageSeverityFlagsEXT severity,
			DebugUtilsMessageTypeFlagsEXT type,
			DebugUtilsMessengerCallbackDataEXT* data,
			void* userData )
		{
			string message = XrUnsafeExtensions.CStringToString( data->Message );
			Console.WriteLine( $"XR: {message}" );
			if ( severity.HasFlag( DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt ) )
			{
				Debugger.Break();
			}

			return XR.False;
		}

		public static unsafe bool CreateDebugLogging()
		{
			Console.WriteLine( "CreateDebugLogging" );

			// Create debug logging
			if ( Xr.TryGetInstanceExtension( null, mInstance, out mDebugUtilApi ) )
			{
				var messengerCreateInfo = new DebugUtilsMessengerCreateInfoEXT
				{
					Type = StructureType.DebugUtilsMessengerCreateInfoExt,

					UserCallback = new( XrDebugLog ),
					UserData = null,

					// I dunno if I'll need this many lol
					MessageSeverities = DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
										DebugUtilsMessageSeverityFlagsEXT.InfoBitExt | DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt,
					MessageTypes = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt     | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
								   DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt | DebugUtilsMessageTypeFlagsEXT.ConformanceBitExt
				};

				return XrCheck( mDebugUtilApi.CreateDebugUtilsMessenger( mInstance, messengerCreateInfo, ref mDebugMessenger ) );
			}

			return false;
		}

		public static unsafe bool CreateSystemId()
		{
			Console.WriteLine( "CreateSystemId" );

			SystemGetInfo systemGetInfo = new()
			{
				Type = StructureType.SystemGetInfo,
				FormFactor = FormFactor.HeadMountedDisplay
			};

			if ( !XrCheck( Xr.GetSystem( mInstance, systemGetInfo, ref mSystemId ), "Failed to get system ID" ) )
			{
				return false;
			}

			SystemProperties systemProperties = new() { Type = StructureType.SystemProperties };
			XrCheck( Xr.GetSystemProperties( mInstance, mSystemId, ref systemProperties ), "Failed to get system properties" );

			uint maxWidth = systemProperties.GraphicsProperties.MaxSwapchainImageWidth;
			uint maxHeight = systemProperties.GraphicsProperties.MaxSwapchainImageHeight;

			Console.WriteLine( $"System: {XrUnsafeExtensions.CStringToString( systemProperties.SystemName )}" );
			Console.WriteLine( $"System ID: {systemProperties.SystemId}" );
			Console.WriteLine( $"System graphics properties: {maxWidth}x{maxHeight}" );

			return true;
		}

		public static void InitSystem()
		{
			Console.WriteLine( "InitSystem" );

			// Instance = OpenXR runtime (e.g. SteamVR, Monado, VDXR, Quest OS...)
			CreateInstance();
			LogInstanceInfo();

			// OpenXR debug logging utils
			CreateDebugLogging();

			// System = headset + controllers + other equipment
			CreateSystemId();
		}

		public static void ShutdownSystem()
		{
			Console.WriteLine( "ShutdownSystem" );

			// Destroy debug logging
			if ( mDebugUtilApi is not null )
			{
				XrCheck( mDebugUtilApi.DestroyDebugUtilsMessenger( mDebugMessenger ) );
			}

			XrCheck( Xr.DestroyInstance( mInstance ), "Failed to destroy instance" );
		}
	}
}
