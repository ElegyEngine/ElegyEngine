
namespace Elegy
{
	public enum ConsoleMessageType
	{
		Info,
		Warning,
		Error,
		Fatal
	}

	public struct ConsoleMessage
	{
		public string Text { get; set; }
		public ConsoleMessageType Type { get; set; }
	}

	public interface IConsoleFrontend
	{
		void Init();

		void Shutdown();

		void OnLog( ref ConsoleMessage message );

		void OnUpdate();

		string Name { get; }
	}
}
