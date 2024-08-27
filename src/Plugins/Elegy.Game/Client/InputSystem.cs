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
			// TODO: load keybinds and stuff

			return true;
		}

		public void Shutdown()
		{

		}

		public void GrabMouse()
		{
			Input.Mouse.Cursor.CursorMode = CursorMode.Hidden;
		}

		public void ReleaseMouse()
		{
			Input.Mouse.Cursor.CursorMode = CursorMode.Normal;
		}
	}
}
