
namespace Elegy.ConsoleFrontends
{
	internal class GodotConsoleFrontend : IConsoleFrontend
	{
		public string Name => "Godot Terminal Console";
		public string Error => string.Empty;
		public bool Initialised { get; private set; } = false;

		public bool Init()
		{
			Initialised = true;
			return true;
		}

		public void Shutdown()
		{
			Initialised = false;
		}

		public void OnLog( string message, ConsoleMessageType type )
		{
			message = message.Replace( "\r", string.Empty );
			if ( message.EndsWith( '\n' ) )
			{
				message = message.TrimEnd( '\n' );
			}

			switch ( type )
			{
				default:
					GD.Print( message );
					break;

				case ConsoleMessageType.Warning:
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					GD.Print( $"[WARNING] {message}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Error:
					System.Console.ForegroundColor = ConsoleColor.Red;
					GD.PrintErr( $"[ERROR] {message}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Fatal:
					System.Console.ForegroundColor = ConsoleColor.Red;
					GD.PrintErr( $"[FATAL] {message}" );
					System.Console.ResetColor();
					break;
			}
		}

		public void OnUpdate()
		{

		}
	}
}
