// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.InputSystem.API;
using Silk.NET.Input;

namespace Game.Client
{
	public class InputSystem
	{
		public bool Init()
		{
			// TODO: keybinds and stuff
			GrabMouse();

			return true;
		}

		public void Shutdown()
		{

		}

		public bool IsMouseGrabbed()
			=> Input.Mouse.Cursor.CursorMode == CursorMode.Raw;

		public void GrabMouse()
		{
			Input.Mouse.Cursor.CursorMode = CursorMode.Raw;
		}

		public void ReleaseMouse()
		{
			Input.Mouse.Cursor.CursorMode = CursorMode.Normal;
		}
	}
}
