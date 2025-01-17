using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Valve.VR;

namespace XrResearch
{
	public static partial class VrTracking
	{
		public enum BodyTrackerType
		{
			Chest,
			Waist,
			LeftAnkle,
			LeftElbow,
			LeftFoot,
			LeftKnee,
			LeftShoulder,
			LeftWrist,
			RightAnkle,
			RightElbow,
			RightFoot,
			RightKnee,
			RightShoulder,
			RightWrist,
			Max
		}

		private static uint[] mTrackerTable = new uint[(int)BodyTrackerType.Max];
		private static BodyTrackerType[] mBodypartTable = new BodyTrackerType[64];

		private static CVRSystem? ovrSystem;
		private static CVRInput? ovrInput;

		private static TrackedDevicePose_t[] mGameDevicePoses = new TrackedDevicePose_t[64];

		// TODO: Read steamvr.vrsettings to obtain the roles directly!
		private static bool LinkTrackerToBodyPart( uint deviceId, string serial )
		{
			BodyTrackerType type = serial.ToLower() switch
			{
				"chest"           => BodyTrackerType.Chest,
				"hips" or "waist" => BodyTrackerType.Waist,

				"left ankle" or "leftankle" or "lankle"          => BodyTrackerType.LeftAnkle,
				"left elbow" or "leftelbow" or "lelbow"          => BodyTrackerType.LeftElbow,
				"left foot" or "leftfoot" or "lfoot"             => BodyTrackerType.LeftFoot,
				"left knee" or "leftknee" or "lknee"             => BodyTrackerType.LeftKnee,
				"left shoulder" or "leftshoulder" or "lshoulder" => BodyTrackerType.LeftShoulder,
				"left wrist" or "leftwrist" or "lwrist"          => BodyTrackerType.LeftWrist,

				"right ankle" or "rightankle" or "rankle"          => BodyTrackerType.RightAnkle,
				"right elbow" or "rightelbow" or "relbow"          => BodyTrackerType.RightElbow,
				"right foot" or "rightfoot" or "rfoot"             => BodyTrackerType.RightFoot,
				"right knee" or "rightknee" or "rknee"             => BodyTrackerType.RightKnee,
				"right shoulder" or "rightshoulder" or "rshoulder" => BodyTrackerType.RightShoulder,
				"right wrist" or "rightwrist" or "rwrist"          => BodyTrackerType.RightWrist,

				_ => BodyTrackerType.Max
			};

			if ( type is BodyTrackerType.Max )
			{
				return false;
			}

			mTrackerTable[(int)type] = deviceId;
			mBodypartTable[deviceId] = type;
			return true;
		}

		public static bool Available => ovrSystem is not null && ovrInput is not null;

		public static bool TrackerAvailable( BodyTrackerType type )
			=> type is not BodyTrackerType.Max && mTrackerTable[(int)type] is not 0U;

		public static bool TrackerActive( BodyTrackerType type )
			=> mGameDevicePoses[mTrackerTable[(int)type]].bPoseIsValid;

		public static Vector3 GetPosition( BodyTrackerType type )
			=> mGameDevicePoses[mTrackerTable[(int)type]].mDeviceToAbsoluteTracking.GetPosition();

		//public static Vector3 GetVelocity( BodyTrackerType type )
		//	=> mGameDevicePoses[mTrackerTable[(int)type]].vVelocity.ToVector3();

		public static bool Init()
		{
			EVRInitError error = EVRInitError.None;
			OpenVR.Init( ref error, EVRApplicationType.VRApplication_Background );

			if ( error is not EVRInitError.None )
			{
				Console.WriteLine( $"EVRInitError: {error}" );
				return false;
			}

			Array.Fill( mTrackerTable, 0U );

			ovrSystem = OpenVR.System;
			ovrInput = OpenVR.Input;

			string headsetName = ovrSystem.GetStringDeviceProperty(
				OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String );
			string serialNumber = ovrSystem.GetStringDeviceProperty(
				OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String );

			Console.WriteLine( $"VR headset system: {headsetName}/{serialNumber}" );

			for ( uint deviceId = 1; deviceId < OpenVR.k_unMaxTrackedDeviceCount; deviceId++ )
			{
				ETrackedDeviceClass deviceClass = ovrSystem.GetTrackedDeviceClass( deviceId );
				if ( deviceClass is ETrackedDeviceClass.Invalid )
				{
					continue;
				}

				string name = ovrSystem.GetStringDeviceProperty( deviceId, ETrackedDeviceProperty.Prop_ActualTrackingSystemName_String );
				string serial = ovrSystem.GetStringDeviceProperty( deviceId, ETrackedDeviceProperty.Prop_SerialNumber_String );

				Console.WriteLine( $"Device {deviceId}: '{name}/{serial}' - {deviceClass}" );
				if ( LinkTrackerToBodyPart( deviceId, serial ) )
				{
					Console.WriteLine( $" * Recognised as a tracker for {mBodypartTable[deviceId]}" );
				}
			}

			return true;
		}

		public static void Update( float delta )
		{
			Debug.Assert( Available );

			VREvent_t vrEvent = default;
			while ( ovrSystem.PollNextEvent( ref vrEvent, (uint)Unsafe.SizeOf<VREvent_t>() ) )
			{
				
			}
			
			ovrSystem.GetDeviceToAbsoluteTrackingPose( ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, mGameDevicePoses );
		}

		public static void Shutdown()
		{
			if ( !Available )
			{
				return;
			}

			OpenVR.Shutdown();
		}
	}
}
