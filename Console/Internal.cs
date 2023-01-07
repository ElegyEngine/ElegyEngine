
namespace Elegy
{
	public static partial class Console
	{
		internal static void SetConsole( Internal.Console console )
		{
			mConsole = console;
		}

		private static Internal.Console mConsole;
	}
}
