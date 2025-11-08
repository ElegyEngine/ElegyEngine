// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

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

		Menu = 1 << 10,
		Tab = 1 << 11,
		Confirm = 1 << 12
	}

	public struct ClientCommand
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
		/// What action buttons are being held down (e.g. primary attack, jump etc.)
		/// </summary>
		public int ActionFlags { get; set; }

		/// <summary>
		/// Packed axis buffer.
		/// </summary>
		public int PackedAxes { get; set; }

		/// <summary>
		/// Input context ID.
		/// </summary>
		public int InputContext { get; set; }

		/// <summary>
		/// Shortcut to check for action flags.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
		public bool HasAction<T>( T flag ) where T : Enum
			=> ((int)(object)flag & ActionFlags) != 0; // Ugly little hack
	}
}
