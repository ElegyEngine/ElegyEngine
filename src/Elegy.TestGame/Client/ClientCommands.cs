// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Client
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
		// Movement direction in world coordinates
		// Not exactly normalised, but doesn't contain speed
		public Vector3 MovementDirection { get; set; }
		// Euler view angles in world coordinates
		public Vector3 ViewAngles { get; set; }
		// What buttons are being held down (e.g. primary attack, jump etc.)
		public ClientActions ActionStates { get; set; }
	}
}
