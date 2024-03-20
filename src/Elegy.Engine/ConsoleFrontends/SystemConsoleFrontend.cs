// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Engine.Interfaces;

namespace Elegy.Engine.ConsoleFrontends
{
	internal class SystemConsoleFrontend : IConsoleFrontend
	{
		public string Name => "System Console";
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
			string messageNoColour = string.Empty;
			for ( int i = 0; i < message.Length; i++ )
			{
				if ( message[i] == '$' && i < message.Length - 1 )
				{
					i += 2;
				}

				messageNoColour += message[i];
			}

			switch ( type )
			{
				default:
					System.Console.WriteLine( messageNoColour );
					break;

				case ConsoleMessageType.Success:
					System.Console.ForegroundColor = ConsoleColor.Green;
					System.Console.WriteLine( $"{messageNoColour}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Warning:
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					System.Console.WriteLine( $"{messageNoColour}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Error:
					System.Console.ForegroundColor = ConsoleColor.Red;
					System.Console.Error.WriteLine( $"{messageNoColour}" );
					System.Console.ResetColor();
					break;

				case ConsoleMessageType.Fatal:
					System.Console.BackgroundColor = ConsoleColor.Red;
					System.Console.ForegroundColor = ConsoleColor.White;
					System.Console.Error.WriteLine( $"[FATAL] {messageNoColour}" );
					System.Console.ResetColor();
					break;
			}
		}

		public void OnUpdate( float delta )
		{

		}
	}
}
