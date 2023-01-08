
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

		public void OnLog( ref ConsoleMessage message )
		{
			switch ( message.Type )
			{
				case ConsoleMessageType.Info:

					message.Text = message.Text.Replace( "\r", string.Empty );
					if ( message.Text.EndsWith( '\n' ) )
					{
						message.Text = message.Text.TrimEnd( '\n' );
					}

					GD.Print( message.Text );
					break;

				case ConsoleMessageType.Warning:
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					GD.Print( $"[WARNING] {message.Text}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Error:
					System.Console.ForegroundColor = ConsoleColor.Red;
					GD.PrintErr( $"[ERROR] {message.Text}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Fatal:
					System.Console.ForegroundColor = ConsoleColor.Red;
					GD.PrintErr( $"[FATAL] {message.Text}" );
					System.Console.ResetColor();
					break;
			}
		}

		public void OnUpdate()
		{

		}
	}
}
