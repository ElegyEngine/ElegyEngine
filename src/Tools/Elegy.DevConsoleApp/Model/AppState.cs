// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.DevConsoleApp.Model
{
	internal class AppState
	{
		public bool IsRunning { get; set; } = true;
		public string InputText { get; set; } = string.Empty;
	}
}
