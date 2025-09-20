// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Input;
using ElInput = Elegy.InputSystem.API.Input;
using ElKey = Silk.NET.Input.Key;
using ElMouseButton = Silk.NET.Input.MouseButton;

namespace Elegy.Avalonia.Input;

/// <summary>Contains methods to get input modifiers.</summary>
internal static class InputModifiersProvider
{
	public static RawInputModifiers GetRawInputModifiers()
	{
		var modifiers = RawInputModifiers.None;

		if ( ElInput.Keyboard.IsKeyPressed( ElKey.AltLeft ) )
			modifiers |= RawInputModifiers.Alt;
		if ( ElInput.Keyboard.IsKeyPressed( ElKey.ControlLeft ) )
			modifiers |= RawInputModifiers.Control;
		if ( ElInput.Keyboard.IsKeyPressed( ElKey.ShiftLeft ) )
			modifiers |= RawInputModifiers.Shift;

		if ( ElInput.Mouse.IsButtonPressed( ElMouseButton.Left ) )
			modifiers |= RawInputModifiers.LeftMouseButton;
		if ( ElInput.Mouse.IsButtonPressed( ElMouseButton.Right ) )
			modifiers |= RawInputModifiers.RightMouseButton;
		if ( ElInput.Mouse.IsButtonPressed( ElMouseButton.Middle ) )
			modifiers |= RawInputModifiers.MiddleMouseButton;
		if ( ElInput.Mouse.IsButtonPressed( ElMouseButton.Button4 ) )
			modifiers |= RawInputModifiers.XButton1MouseButton;
		if ( ElInput.Mouse.IsButtonPressed( ElMouseButton.Button5 ) )
			modifiers |= RawInputModifiers.XButton2MouseButton;

		return modifiers;
	}

	public static KeyModifiers GetKeyModifiers()
	{
		var modifiers = KeyModifiers.None;

		if ( ElInput.Keyboard.IsKeyPressed( ElKey.AltLeft ) )
			modifiers |= KeyModifiers.Alt;
		if ( ElInput.Keyboard.IsKeyPressed( ElKey.ControlLeft ) )
			modifiers |= KeyModifiers.Control;
		if ( ElInput.Keyboard.IsKeyPressed( ElKey.ShiftLeft ) )
			modifiers |= KeyModifiers.Shift;

		return modifiers;
	}
}
