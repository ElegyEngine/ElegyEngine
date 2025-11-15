// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Shared.Input.Actions
{
	[Flags]
	public enum PlayerActions
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
}
