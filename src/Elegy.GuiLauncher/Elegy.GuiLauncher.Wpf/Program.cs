
using System;

namespace Elegy.GuiLauncher.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
			=> EntryPoint.RunApplication( args, Eto.Platforms.Wpf );
	}
}
