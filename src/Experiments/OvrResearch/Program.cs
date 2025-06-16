using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Valve.VR;

namespace OvrResearch;

public static class Program
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

	private static CVRSystem ovrSystem;
	private static CVRInput ovrInput;
	private static CVRCompositor ovrCompositor;

	private static ulong mActionSet;
	private static ulong mActionSetFbt;
	private static ulong mHandPoseAction;
	private static ulong mPrimaryButtonAction;
	private static ulong mSecondaryButtonAction;
	private static ulong mLeftHandPathHandle;
	private static ulong mRightHandPathHandle;
	private static InputPoseActionData_t mLeftHandPoseData;
	private static InputPoseActionData_t mRightHandPoseData;

	private static TrackedDevicePose_t[] mGameDevicePoses = new TrackedDevicePose_t[64];

	public static string GetStringDeviceProperty( uint deviceId, ETrackedDeviceProperty property )
	{
		StringBuilder sb = new();
		ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
		ovrSystem.GetStringTrackedDeviceProperty( deviceId, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error );
		if ( error is not ETrackedPropertyError.TrackedProp_Success )
		{
			Console.WriteLine( $"GetStringDeviceProperty: error '{error}'" );
			Debugger.Break();
			return error.ToString();
		}

		return sb.ToString();
	}

	public static Matrix4x4 ToMatrix( this HmdMatrix34_t m )
	{
		return new Matrix4x4
		{
			M11 = m.m0,
			M12 = m.m4,
			M13 = -m.m8,
			M14 = 0,

			M21 = m.m1,
			M22 = m.m5,
			M23 = -m.m9,
			M24 = 0,

			M31 = -m.m2,
			M32 = -m.m6,
			M33 = m.m10,
			M34 = 0,

			M41 = m.m3,
			M42 = m.m7,
			M43 = -m.m11,
			M44 = 1
		};
	}

	public static Vector3 GetPosition( this HmdMatrix34_t m )
	{
		return new Vector3( m.m3, m.m7, -m.m11 );
	}

	public static bool Check( this EVRInputError error )
	{
		if ( error is EVRInputError.None )
		{
			return true;
		}

		Console.WriteLine( $"OpenVR input error: {error}" );
		Debugger.Break();
		return false;
	}

	// TODO: Read steamvr.vrsettings to obtain the roles directly!
	public static bool LinkTrackerToBodyPart( uint deviceId, string serial )
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

	public static void Main( string[] args )
	{
		EVRInitError error = EVRInitError.None;
		OpenVR.Init( ref error, EVRApplicationType.VRApplication_Background );

		if ( error is not EVRInitError.None )
		{
			Console.WriteLine( $"EVRInitError: {error}" );
			return;
		}

		ovrSystem = OpenVR.System;
		ovrInput = OpenVR.Input;
		ovrCompositor = OpenVR.Compositor;

		string headsetName = GetStringDeviceProperty(
			OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_TrackingSystemName_String );
		string displayName = GetStringDeviceProperty(
			OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SerialNumber_String );

		Console.WriteLine( $"Headset name:  {headsetName}" );
		Console.WriteLine( $"Serial number: {displayName}" );

		ovrInput.SetActionManifestPath( Path.GetFullPath( "or_actions.json" ) ).Check();

		ovrInput.GetActionSetHandle( "/actions/elegy", ref mActionSet ).Check();
		ovrInput.GetInputSourceHandle( OpenVR.k_pchPathUserHandLeft, ref mLeftHandPathHandle ).Check();
		ovrInput.GetInputSourceHandle( OpenVR.k_pchPathUserHandRight, ref mRightHandPathHandle ).Check();
		ovrInput.GetActionHandle( "/actions/elegy/in/hand_pose", ref mHandPoseAction ).Check();
		ovrInput.GetActionHandle( "/actions/elegy/in/primary", ref mPrimaryButtonAction ).Check();
		ovrInput.GetActionHandle( "/actions/elegy/in/secondary", ref mSecondaryButtonAction ).Check();

		for ( uint deviceId = 1; deviceId < OpenVR.k_unMaxTrackedDeviceCount; deviceId++ )
		{
			ETrackedDeviceClass deviceClass = ovrSystem.GetTrackedDeviceClass( deviceId );
			if ( deviceClass is ETrackedDeviceClass.Invalid )
			{
				continue;
			}

			string name = GetStringDeviceProperty( deviceId, ETrackedDeviceProperty.Prop_ActualTrackingSystemName_String );
			string serial = GetStringDeviceProperty( deviceId, ETrackedDeviceProperty.Prop_SerialNumber_String );

			Console.WriteLine( $"Device {deviceId}: '{name}/{serial}' - {deviceClass}" );
			if ( LinkTrackerToBodyPart( deviceId, serial ) )
			{
				Console.WriteLine( $" * Recognised as a tracker for {mBodypartTable[deviceId]}" );
			}
		}

		void PrintTrackedDevice( in VREvent_t vrEvent, string action )
		{
			string name = GetStringDeviceProperty( vrEvent.trackedDeviceIndex,
				ETrackedDeviceProperty.Prop_TrackingSystemName_String );
			Console.WriteLine( $"Tracked device {action}: '{name}' ({vrEvent.trackedDeviceIndex})" );
		}

		int time = 10000;
		while ( time > 0 )
		{
			VREvent_t vrEvent = new();
			while ( ovrSystem.PollNextEvent( ref vrEvent, (uint)Unsafe.SizeOf<VREvent_t>() ) )
			{
				EVREventType eventType = (EVREventType)vrEvent.eventType;

				if ( eventType is EVREventType.VREvent_TrackedDeviceActivated )
				{
					PrintTrackedDevice( vrEvent, "activated" );
				}
				else if ( eventType is EVREventType.VREvent_TrackedDeviceDeactivated )
				{
					PrintTrackedDevice( vrEvent, "deactivated" );
				}
				else if ( eventType is EVREventType.VREvent_TrackedDeviceUpdated )
				{
					PrintTrackedDevice( vrEvent, "updated" );
				}
				else if ( eventType is EVREventType.VREvent_Input_TrackerActivated )
				{
					PrintTrackedDevice( vrEvent, "activated (tracker!)" );
				}
				else if ( eventType is EVREventType.VREvent_PropertyChanged )
				{
					PrintTrackedDevice( vrEvent, "changed properties" );
				}
			}

			VRActiveActionSet_t actionSet = new()
			{
				ulActionSet = mActionSet,
				ulRestrictedToDevice = 0,
			};

			VRActiveActionSet_t actionSetFbt = new()
			{
				ulActionSet = mActionSetFbt,
				ulRestrictedToDevice = 0,
			};

			ovrInput.UpdateActionState( [actionSet, actionSetFbt], (uint)Unsafe.SizeOf<VRActiveActionSet_t>() )
				.Check();

			ovrInput.GetPoseActionDataForNextFrame(
					mHandPoseAction, ETrackingUniverseOrigin.TrackingUniverseStanding,
					ref mLeftHandPoseData, (uint)Unsafe.SizeOf<InputPoseActionData_t>(), mLeftHandPathHandle )
				.Check();

			ovrInput.GetPoseActionDataForNextFrame(
					mHandPoseAction, ETrackingUniverseOrigin.TrackingUniverseStanding,
					ref mRightHandPoseData, (uint)Unsafe.SizeOf<InputPoseActionData_t>(), mRightHandPathHandle )
				.Check();

			ovrSystem.GetDeviceToAbsoluteTrackingPose( ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, mGameDevicePoses );

			if ( mTrackerTable[(int)BodyTrackerType.Chest] is not OpenVR.k_unTrackedDeviceIndexInvalid )
			{
				ref var pose = ref mGameDevicePoses[mTrackerTable[(int)BodyTrackerType.Chest]];
				if ( pose.bPoseIsValid )
				{
					var position = pose.mDeviceToAbsoluteTracking.GetPosition();
					Console.WriteLine( $"Chest position: {position}" );
				}
			}

			Thread.Sleep( 30 );
			time--;
		}

		OpenVR.Shutdown();
	}
}
