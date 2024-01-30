
using Eto.Forms;
using Silk.NET.Windowing;
using System;

namespace Elegy.GuiLauncher.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
		{


			new Application( Eto.Platforms.Wpf ).Run( new MainForm() );
		}
	}
}
