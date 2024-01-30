
using Eto.Forms;
using System;

namespace Elegy.GuiLauncher.Gtk
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
		{
			new Application( Eto.Platforms.Gtk ).Run( new MainForm() );
		}
	}
}
