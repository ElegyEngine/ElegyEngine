using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Valve.VR;

namespace XrResearch;

public static unsafe class OpenVrExtensions
{
	public static string GetStringDeviceProperty( this CVRSystem ovrSystem, uint deviceId, ETrackedDeviceProperty property )
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
}
