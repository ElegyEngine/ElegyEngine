// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System;

namespace Elegy.GuiLauncher.Gtk
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args ) 
			=> EntryPoint.RunApplication( args, Eto.Platforms.Gtk );
	}
}
