
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
