
using Eto.Forms;
using System;

namespace Elegy.GuiLauncher.Mac
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
		{
			new Application( Eto.Platforms.Mac64 ).Run( new MainForm() );
		}
	}
}
