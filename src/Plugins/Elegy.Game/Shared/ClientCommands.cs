// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared
{
	[Flags]
	public enum ClientActions
	{
		PrimaryAttack = 1 << 0,
		SecondaryAttack = 1 << 1,
		TertiaryAttack = 1 << 2,

		Sprint = 1 << 3,
		Use = 1 << 4,
		Reload = 1 << 5,
		Flashlight = 1 << 6,

		LeanLeft = 1 << 7,
		LeanRight = 1 << 8,
		LeanForward = 1 << 9,

		Escape = 1 << 10,
		Tab = 1 << 11,
		Enter = 1 << 12
	}

	public struct ClientCommands
	{
		/// <summary>
		/// Time of recording this client command.
		/// </summary>
		public ulong Time { get; set; }

		/// <summary>
		/// Movement direction in world coordinates.
		/// Not exactly normalised, but doesn't contain speed.
		/// </summary>
		public Vector3 MovementDirection { get; set; }

		/// <summary>
		/// Euler view angles in world coordinates.
		/// </summary>
		public Vector3 ViewAngles { get; set; }

		/// <summary>
		/// What buttons are being held down (e.g. primary attack, jump etc.)
		/// </summary>
		public ClientActions ActionStates { get; set; }
	}
}
