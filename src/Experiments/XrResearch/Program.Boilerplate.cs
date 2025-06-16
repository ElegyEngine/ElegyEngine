using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Maths;
using Silk.NET.OpenXR;
using Silk.NET.OpenXR.Extensions.EXT;
using Silk.NET.OpenXR.Extensions.HTCX;
using Silk.NET.OpenXR.Extensions.KHR;
using XrAction = Silk.NET.OpenXR.Action;

namespace XrResearch
{
	public static partial class Program
	{
		public static XR Xr;

		public static ulong XrMakeVersion( ulong major, ulong minor, ulong patch )
		{
			return ((major & 0xffffUL) << 48) | ((minor & 0xffffUL) << 32) | (patch & 0xffffffffUL);
		}

		public static ulong XrVersionMajor( ulong version ) => (version >> 48) & 0xffffUL;
		public static ulong XrVersionMinor( ulong version ) => (version >> 32) & 0xffffUL;
		public static ulong XrVersionPatch( ulong version ) => version         & 0xffffffffUL;

		public static bool XrCheck( Result result, string message = "", bool dontBreak = false )
		{
			if ( result is not Result.Success )
			{
				if ( Debugger.IsAttached && !dontBreak )
				{
					Debugger.Break();
				}

				Console.WriteLine( $"XR runtime error: {message} ({result})" );

				return false;
			}

			return true;
		}
	}
}
