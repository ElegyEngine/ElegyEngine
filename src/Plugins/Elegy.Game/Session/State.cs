// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Session
{
	public enum GameSessionState
	{
		Disconnected,
		Authenticating,
		Connecting,
		Joining,
		Connected
	}

	public class State
	{
		public bool Init()
		{
			return true;
		}

		public void Shutdown()
		{

		}
	}
}
