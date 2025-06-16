using Silk.NET.OpenXR;

namespace XrResearch
{
	public static partial class Program
	{
		private static Session mSession;

		private static bool mIsRunning;
		private static bool mSessionIsRunning;
		private static SessionState mSessionState = SessionState.Unknown;

		private static Space mReferenceSpace;

		public static unsafe bool CreateSession()
		{
			Console.WriteLine( "CreateSession" );

			// Create an XR session
			GraphicsBinding xrGraphicsBinding = mGraphicsApi?.GetGraphicsBinding() ?? default;
			SessionCreateInfo sessionCreateInfo = new()
			{
				Type = StructureType.SessionCreateInfo,
				CreateFlags = 0,
				SystemId = mSystemId,
				Next = mGraphicsApi is null ? null : &xrGraphicsBinding
			};

			return XrCheck( Xr.CreateSession( mInstance, sessionCreateInfo, ref mSession ), "Failed to create session" );
		}

		private static void CreateReferenceSpace()
		{
			Console.WriteLine( "CreateReferenceSpace" );

			ReferenceSpaceCreateInfo createInfo = new()
			{
				Type = StructureType.ReferenceSpaceCreateInfo,
				ReferenceSpaceType = ReferenceSpaceType.LocalFloor,
				PoseInReferenceSpace = new Posef
				{
					Position = new( 0, 0, 0 ),
					Orientation = new( 0, 0, 0, 1 )
				}
			};

			XrCheck( Xr.CreateReferenceSpace( mSession, createInfo, ref mReferenceSpace ) );
		}

		public static void InitSession()
		{
			Console.WriteLine( "InitSession" );

			// Session = XR application state
			CreateSession();
			CreateReferenceSpace();

			mIsRunning = true;
		}

		public static void ShutdownSession()
		{
			Console.WriteLine( "ShutdownSession" );

			XrCheck( Xr.DestroySpace( mReferenceSpace ) );
			XrCheck( Xr.DestroySession( mSession ) );
		}
	}
}
