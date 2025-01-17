using System.Numerics;
using Silk.NET.OpenXR;
using Veldrid;

namespace XrResearch
{
	public static partial class Program
	{
		private static GraphicsDevice mDevice;
		private static CommandList mCommands;
		
		public static void Main( string[] args )
		{
			Xr = XR.GetApi();

			InitSystem();
			InitInput();

			if ( GetVulkanVrExtension() )
			{
				mDevice = GraphicsDevice.CreateVulkan( new()
				{
					HasMainSwapchain = false,
					Debug = false
				}, new VulkanDeviceOptions
				{
					DeviceExtensions = GetVulkanVrDeviceExtensions(),
					InstanceExtensions = GetVulkanVrInstanceExtensions(),
					GetPhysicalDevice = VulkanVrGetPhysicalDevice
				} );

				mGraphicsApi = new VeldridVulkanVrApi( mDevice );
				mCommands = mDevice.ResourceFactory.CreateCommandList();
			}

			InitSession();
			InitGraphics();

			PostInitInput();

			bool tracking = VrTracking.Init();

			// The loop(tm)
			int coordinateTimer = 100;
			while ( mIsRunning )
			{
				LoopInput();

				if ( mSessionIsRunning )
				{
					RenderFrame();
				}

				// For now X exits the application
				if ( mSessionIsRunning && mButtonXState.CurrentState != 0 )
				{
					XrCheck( Xr.RequestExitSession( mSession ) );
				}

				Thread.Sleep( 10 );

				if ( --coordinateTimer == 0 )
				{
					coordinateTimer = 100;

					Console.WriteLine( "Tracker coordinates:" );
					Vector3 v = new( mLeftHandPose.Position.X, mLeftHandPose.Position.Y, mLeftHandPose.Position.Z );
					Console.WriteLine( $"OpenXR: LHAND: {v}" );
					v = new( mRightHandPose.Position.X, mRightHandPose.Position.Y, mRightHandPose.Position.Z );
					Console.WriteLine( $"OpenXR: RHAND: {v}" );

					if ( VrTracking.Available )
					{
						VrTracking.Update( 0.016f );

						if ( VrTracking.TrackerActive( VrTracking.BodyTrackerType.Waist ) )
						{
							v = VrTracking.GetPosition( VrTracking.BodyTrackerType.Waist );
							Console.WriteLine( $"OpenVR: WAIST: {v}" );
						}

						if ( VrTracking.TrackerActive( VrTracking.BodyTrackerType.Chest ) )
						{
							v = VrTracking.GetPosition( VrTracking.BodyTrackerType.Chest );
							Console.WriteLine( $"OpenVR: CHEST: {v}" );
						}
					}
				}
			}

			// Destroy everything
			ShutdownSession();
			ShutdownGraphics();
			ShutdownInput();
			if ( tracking ) VrTracking.Shutdown();
			ShutdownSystem();
		}
	}
}
