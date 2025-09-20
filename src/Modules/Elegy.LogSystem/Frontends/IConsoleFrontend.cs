// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces;

namespace Elegy.LogSystem.Frontends
{
	/// <summary>
	/// Submit frontend interface. Submit frontends are essentially
	/// apps or widgets that display the engine's logged console messages.
	/// </summary>
	public interface IConsoleFrontend : IPlugin
	{
		/// <summary>
		/// A message is being logged. Write it down.
		/// Submit frontends should never use methods from Elegy.Submit,
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
