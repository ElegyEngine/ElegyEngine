// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public interface IConsoleFrontend : IPlugin
	{
		/// <summary>
		/// A message is being logged. Write it down.
		/// Console frontends should never use methods from Elegy.Console,
		/// else you get an infinite loop.
		/// </summary>
		void OnLog( string message, ConsoleMessageType type, float timeSubmitted );

		/// <summary>
		/// Called every frame to update the console frontend,
		/// in case it may be interactive or have some animations.
		/// </summary>
		void OnUpdate( float delta );
	}
}
