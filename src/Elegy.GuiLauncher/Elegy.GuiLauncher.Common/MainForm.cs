
using Eto.Drawing;
using Eto.Forms;
using System.Runtime.InteropServices;

namespace Elegy.GuiLauncher
{
	internal class FormLogger : IConsoleFrontend
	{
		private TextArea mText;

		public string Name => "Eto.Forms Logger";

		public string Error { get; set; } = string.Empty;

		public bool Initialised { get; set; }

		public FormLogger( TextArea text )
		{
			mText = text;
		}

		public bool Init()
		{
			Initialised = true;
			return true;
		}

		public void OnLog( string message, ConsoleMessageType type, float timeSubmitted )
		{
			string messageNoColour = string.Empty;
			for ( int i = 0; i < message.Length; i++ )
			{
				if ( message[i] == '$' && i < message.Length - 1 )
				{
					i += 2;
				}

				messageNoColour += message[i];
			}

			mText.Text += $"{messageNoColour}\n";
		}

		public void OnUpdate( float delta )
		{

		}

		public void Shutdown()
		{

		}
	}

	internal class FormApp : IApplication
	{
		public string Name => "Form App";

		public string Error => string.Empty;

		public bool Initialised { get; private set; }

		public bool Init()
		{
			Initialised = true;
			return true;
		}

		public int Clicks = 4;

		public bool RunFrame( float delta )
		{
			Console.Log( $"Remaining clicks: {Clicks + 1}" );
			Clicks--;

			return Clicks > 0;
		}

		public void Shutdown()
		{
			Initialised = false;
		}

		public bool Start()
		{
			return true;
		}
	}

	public partial class MainForm : Form
	{
		private SilkWindow mSilkWindow;
		private FormLogger mLogger;
		private TextArea mTextArea;
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
			MinimumSize = new( 1280, 960 );

			//mSilkWindow = new SilkWindow()
			//{
			//	Size = new Size( 800, 600 )
			//};

			mTextArea = new()
			{
				Size = new( 1200, 900 ),
				ReadOnly = true
			};

			mLogger = new( mTextArea );

			Content = new StackLayout
			{
				Padding = 10,
				Items =
				{
					"Engine output:",
					mTextArea
				}
			};

			ToolBar = new ToolBar
			{
				Items =
				{
					CreateButton( "Start engine", (sender, e) =>
					{
						if ( !mEngine.Init( mLogger ) )
						{
							MessageBox.Show( mEngine.ShutdownReason );
							return;
						}

						Plugins.RegisterPlugin( new FormApp() );
					} ),

					CreateButton( "Run engine", (sender, e) =>
					{
						mEngine.Run();
					} )
				}
			};
		}
	}

	public static class EntryPoint
	{
		public static void RunApplication( string[] args, string platform )
		{
			Engine engine = new( args, null );

			new Application( platform )
				.Run( new MainForm( engine ) );
		}
	}
}
