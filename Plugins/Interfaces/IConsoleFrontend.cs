
namespace Elegy
{
	public interface IConsoleFrontend : IPlugin
	{
		/// <summary>
		/// A message is being logged. Write it down.
		/// </summary>
		void OnLog( string message, ConsoleMessageType type );

		/// <summary>
		/// Called every frame to update the console frontend,
		/// in case it may be interactive or have some animations.
		/// </summary>
		void OnUpdate();
	}
}
