
using Eto.Forms;
using System;

namespace Elegy.GuiLauncher.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
		{
			Engine engine = new( args, null );

			new Application( Eto.Platforms.Wpf )
				.Run( new MainForm( engine ) );
		}
	}
}
