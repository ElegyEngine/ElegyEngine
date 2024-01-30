
using Eto.Drawing;
using Eto.Forms;
using System.Runtime.InteropServices;

namespace Elegy.GuiLauncher
{
	public partial class MainForm : Form
	{
		private SilkWindow mSilkWindow;
		private Engine mEngine;

		private Command CreateButton( string name, EventHandler<EventArgs> action )
		{
			Command command = new()
			{
				MenuText = name,
				ToolBarText = name
			};

			command.Executed += action;

			return command;
		}

		public MainForm( Engine engine )
		{
			mEngine = engine;

			Title = "My Eto Form";
			MinimumSize = new Size( 1024, 768 );

			mSilkWindow = new SilkWindow()
			{
				Size = new Size( 800, 600 )
			};

			Content = new StackLayout
			{
				Padding = 10,
				Items =
				{
					"Hello World!",
					mSilkWindow
				}
			};

			// create a few commands that can be used for the menu and toolbar
			var clickMe = new Command { MenuText = "Click Me!", ToolBarText = "Click Me!" };
			clickMe.Executed += ( sender, e ) => MessageBox.Show( this, "I was clicked!" );

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new SubMenuItem { Text = "&File", Items = { clickMe } },
				},
				ApplicationItems =
				{
					new ButtonMenuItem { Text = "&Preferences..." }
				},
				QuitItem = CreateButton( "Quit", (sender, e) => Application.Instance.Quit() ),
				AboutItem = CreateButton( "About...", (sender, e) => new AboutDialog().ShowDialog( this ) )
			};

			// create toolbar			
			ToolBar = new ToolBar
			{
				Items =
				{
					CreateButton( "Start engine", (sender, e) =>
					{
						if ( !mEngine.Init() )
						{
							MessageBox.Show( mEngine.ShutdownReason );
							return;
						}
					} ),

					CreateButton( "Run engine", (sender, e) =>
					{
						mEngine.Run();
					} )
				}
			};
		}
	}
}
