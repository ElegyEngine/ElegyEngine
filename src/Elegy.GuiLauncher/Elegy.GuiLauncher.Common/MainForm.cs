
using Elegy.Rendering;
using Eto.Forms;
using Silk.Eto;

using SilkWindow = Silk.NET.Windowing.Window;
using SilkInput = Silk.NET.Input.InputWindowExtensions;

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
			return true;
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
		private SilkSurface mSurface;
		private FormLogger mLogger;
		private TextArea mTextArea;
		private Label mStatusLabel;
		private Engine mEngine;

		private IView? mRenderView = null;

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

			mStatusLabel = new()
			{
				Text = "FPS:"
			};

			mSurface = new()
			{
				Size = new( 1280, 720 )
			};

			mTextArea = new()
			{
				Size = new( 1280, 120 ),
				ReadOnly = true
			};

			mLogger = new( mTextArea );

			Content = new StackLayout
			{
				Padding = 5,
				Items =
				{
					mStatusLabel,
					mSurface,
					"Engine output:",
					mTextArea
				},
				Size = new( -1, -1 )
			};

			ToolBar = new ToolBar
			{
				Items =
				{
					CreateButton( "Restart engine", (sender, e) =>
					{
						if ( !mEngine.Init( false, mLogger ) )
						{
							MessageBox.Show( mEngine.ShutdownReason );
							return;
						}

						Core.AddWindow( mSurface );
						mRenderView = Render.Instance.CreateView( mSurface );

						Plugins.RegisterPlugin( new FormApp() );
					} ),

					CreateButton( "Shutdown engine", (sender, e) =>
					{
						
					} )
				}
			};

			mSurface.LoadComplete += OnLoad;
		}

		private void OnLoad( object? sender, EventArgs e )
		{
			if ( !mEngine.Init( false, mLogger ) )
			{
				MessageBox.Show( mEngine.ShutdownReason );
				return;
			}

			Core.AddWindow( mSurface );
			mRenderView = Render.Instance.CreateView( mSurface );

			// Just to give the engine something to do
			Plugins.RegisterPlugin( new FormApp() );

			mLastTime = Core.Seconds;
			mSurface.Draw += OnRender;
		}

		private double mAverageFps = 60.0;
		private double mLastTime = 0.0;
		private double mVerticalSyncTimer = 1.0 / 120.0;
		private void OnRender( object? sender, EventArgs e )
		{
			double deltaTime = Core.Seconds - mLastTime;

			mVerticalSyncTimer -= deltaTime;
			if ( mVerticalSyncTimer > 0.0 )
			{
				return;
			}

			mVerticalSyncTimer = 1.0 / 60.0;
			mEngine.Update( (float)deltaTime );

			if ( mRenderView.RenderSize.X > 0.0f )
			{
				mEngine.RenderFrame( mRenderView );
			}
			else
			{
				mSurface.ForceResizeEvent();
				mRenderView.RenderSize = new( mSurface.RenderWidth, mSurface.RenderHeight );
			}

			mLastTime = Core.Seconds;

			mAverageFps = mAverageFps * 0.9 + (1.0 / deltaTime) * 0.1;
			mStatusLabel.Text = string.Format( "FPS: {0:F}", mAverageFps );
		}
	}

	public static class EntryPoint
	{
		public static void RunApplication( string[] args, string platform )
		{
			Engine engine = new( args, null );

			SilkWindow.Add( new EtoWindowPlatform() );
			SilkInput.Add( new EtoInputPlatform() );

			new Application( platform )
				.Run( new MainForm( engine ) );
		}
	}
}
