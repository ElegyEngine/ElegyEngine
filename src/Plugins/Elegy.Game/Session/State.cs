// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Session
{
	/// <summary>
	/// Client connection state.
	/// </summary>
	public enum GameSessionState
	{
		/// <summary>
		/// Client is disconnected.
		/// </summary>
		Disconnected,

		/// <summary>
		/// Client initiated a connection.
		/// </summary>
		Connecting,

		/// <summary>
		/// Client is authenticating, e.g. via password.
		/// </summary>
		Authenticating,

		/// <summary>
		/// Client has some missing assets, so they are being
		/// downloaded, either directly from the server or
		/// via a UGC platform (e.g. Steam Workshop).
		/// </summary>
		Downloading,

		/// <summary>
		/// Client is loading assets and joining the game session.
		/// </summary>
		Joining,

		/// <summary>
		/// Client has fully connected and is playing.
		/// </summary>
		Connected
	}
}
