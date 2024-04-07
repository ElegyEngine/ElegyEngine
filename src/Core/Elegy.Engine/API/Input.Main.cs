// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Input;

namespace Elegy.Engine.API
{
	/// <summary>
	/// Simple access to input functions. Key presses, mouse coordinates etc.
	/// </summary>
	public static partial class Input
	{
		/// <summary>
		/// Gets the current input state. It can change between windows.
		/// </summary>
		public static IInputContext State => Core.GetCurrentInputContext();

		/// <summary>
		/// Gets the current keyboard.
		/// </summary>
		public static IKeyboard Keyboard => State.Keyboards[0];

		/// <summary>
		/// Gets the current mouse.
		/// </summary>
		public static IMouse Mouse => State.Mice[0];

		/// <summary>
		/// Gets a list of active gamepads. Gamepads are simple controllers with buttons.
		/// </summary>
		public static IReadOnlyList<IGamepad> Gamepads => State.Gamepads;

		/// <summary>
		/// Gets a list of active joysticks. Joysticks are gamepads with analogue
		/// sticks and other features.
		/// </summary>
		public static IReadOnlyList<IJoystick> Joysticks => State.Joysticks;

		/// <summary>
		/// Registers an <paramref name="action"/> to execute when a device connects/disconnects.
		/// </summary>
		public static void OnConnectionChanged( Action<IInputDevice, bool> action )
			=> State.ConnectionChanged += action;
	}
}
