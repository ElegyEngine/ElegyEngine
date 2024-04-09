// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Input;

namespace Elegy.PlatformSystem
{
	/// <summary>
	/// Dummy input context for headless applications. E.g. dedicated servers.
	/// </summary>
	internal class InputContextNull : IInputContext
	{
		public nint Handle => 0;
		public IReadOnlyList<IGamepad> Gamepads => Array.Empty<IGamepad>();
		public IReadOnlyList<IJoystick> Joysticks => Array.Empty<IJoystick>();
		public IReadOnlyList<IKeyboard> Keyboards => Array.Empty<IKeyboard>();
		public IReadOnlyList<IMouse> Mice => Array.Empty<IMouse>();
		public IReadOnlyList<IInputDevice> OtherDevices => Array.Empty<IInputDevice>();

		public event Action<IInputDevice, bool>? ConnectionChanged;

		public void Dispose()
		{

		}
	}
}
