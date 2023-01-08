
namespace Elegy
{
	public static partial class Console
	{
		public static bool AddFrontend( IConsoleFrontend frontend )
			=> mConsole.AddFrontend( frontend );
	}
}
