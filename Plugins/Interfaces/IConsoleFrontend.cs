
namespace Elegy
{
	public interface IConsoleFrontend : IPlugin
	{
		void OnLog( ref ConsoleMessage message );
		void OnUpdate();
	}
}
