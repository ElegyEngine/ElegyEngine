// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

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

		public void OnLog( string message, ConsoleMessageType type, float timeSubmitted )
		{
			message = message.Replace( "\r", string.Empty );
			if ( message.EndsWith( '\n' ) )
			{
				message = message.TrimEnd( '\n' );
			}

			switch ( type )
			{
				default:
					GD.Print( messageNoColour );
					break;

				case ConsoleMessageType.Success:
					System.Console.ForegroundColor = ConsoleColor.Green;
					GD.Print( $"{messageNoColour}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Warning:
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					GD.Print( $"{messageNoColour}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Error:
					System.Console.ForegroundColor = ConsoleColor.Red;
					GD.PrintErr( $"{messageNoColour}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Fatal:
					System.Console.BackgroundColor = ConsoleColor.Red;
					System.Console.ForegroundColor = ConsoleColor.White;
					GD.PrintErr( $"[FATAL] {messageNoColour}" );
					System.Console.ResetColor();
					break;
			}
		}

		public void OnUpdate( float delta )
		{

		}
	}
}
