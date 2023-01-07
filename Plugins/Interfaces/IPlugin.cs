
namespace Elegy
{
	public interface IPlugin
	{
		bool Init();
		void Shutdown();

		string Name { get; }
		string Error { get; }
		bool Initialised { get; }
	}
}
