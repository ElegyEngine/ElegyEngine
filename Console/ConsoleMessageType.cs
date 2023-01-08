
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
}
