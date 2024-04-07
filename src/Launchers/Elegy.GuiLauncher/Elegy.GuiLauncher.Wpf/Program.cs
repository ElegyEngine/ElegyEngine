// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System;

namespace Elegy.GuiLauncher.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
			=> EntryPoint.RunApplication( args, Eto.Platforms.Wpf, () =>
			{
				// Do some WPF-specific hackery here, I dunno
				// Load a dark theme .xaml?
			} );
	}
}
