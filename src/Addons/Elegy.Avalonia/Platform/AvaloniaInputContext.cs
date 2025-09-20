// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Input;
using Silk.NET.Input;
using Silk.NET.Windowing;
using IInputDevice = Silk.NET.Input.IInputDevice;
using Key = Silk.NET.Input.Key;

namespace Elegy.Avalonia.Platform;

// TODO: Figure out how to pass input from Avalonia to Elegy

public class AvaloniaKeyboardAdapter : IKeyboard
{
	public string Name { get; }
	public int Index { get; }
	public bool IsConnected { get; }

	public bool IsKeyPressed( Key key )
	{
		throw new NotImplementedException();
	}

	public bool IsScancodePressed( int scancode )
	{
		throw new NotImplementedException();
	}

	public void BeginInput()
	{
		throw new NotImplementedException();
	}

	public void EndInput()
	{
		throw new NotImplementedException();
	}

	public IReadOnlyList<Key> SupportedKeys { get; }
	public string ClipboardText { get; set; }
	public event Action<IKeyboard, Key, int>? KeyDown;
	public event Action<IKeyboard, Key, int>? KeyUp;
	public event Action<IKeyboard, char>? KeyChar;
}

public class AvaloniaInputContext : IInputContext
{
	public AvaloniaInputContext()
	{
		Keyboard = new();
		Keyboards = [Keyboard];
	}

	public void Dispose()
	{
	}

	#region Input handlers

	public void OnKeyDown( KeyEventArgs e )
	{
		
	}

	public void OnKeyUp( KeyEventArgs e )
	{
		
	}

	#endregion

	public IntPtr Handle => 0;

	public AvaloniaKeyboardAdapter Keyboard { get; }
	public IReadOnlyList<IKeyboard> Keyboards { get; }

	public IReadOnlyList<IGamepad> Gamepads => [];
	public IReadOnlyList<IJoystick> Joysticks => [];
	public IReadOnlyList<IMouse> Mice => [];
	public IReadOnlyList<IInputDevice> OtherDevices => [];
	public event Action<IInputDevice, bool>? ConnectionChanged;
}

public class AvaloniaInputPlatform : IInputPlatform
{
	public bool IsApplicable( IView view )
		=> view is SilkWindowAdapter;

	public IInputContext CreateInput( IView view )
		=> new AvaloniaInputContext();
}
