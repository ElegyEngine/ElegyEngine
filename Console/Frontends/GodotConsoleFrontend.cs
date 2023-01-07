
namespace Elegy.ConsoleFrontends
{
	internal class GodotConsoleFrontend : IConsoleFrontend
	{
		public string Name => "Godot Terminal Console";

		public void Init()
		{
		}

		public void OnLog( ref ConsoleMessage message )
		{
			switch ( message.Type )
			{
				case ConsoleMessageType.Info:
					if ( message.Text.EndsWith( '\n' ) )
					{
						message.Text = message.Text.TrimEnd( '\n' );
					}
					GD.Print( message.Text );
					break;

				case ConsoleMessageType.Warning:
					GD.Print( $"WARNING: {message.Text}" );
					break;

				case ConsoleMessageType.Error:
					GD.PrintErr( $"ERROR: {message.Text}" );
					break;

				case ConsoleMessageType.Fatal:
					GD.PrintErr( $"FATAL: {message.Text}" );
					break;
			}
		}

		public void OnUpdate()
		{

		}

		public void Shutdown()
		{

		}
	}
}
