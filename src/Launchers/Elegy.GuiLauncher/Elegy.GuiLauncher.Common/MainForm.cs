// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Interfaces;
using Elegy.ConsoleSystem;
using Elegy.ConsoleSystem.Frontends;
using Elegy.Framework;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces.Rendering;
using Elegy.PluginSystem.API;

using Eto;
using Eto.Forms;
using Eto.Drawing;
using Silk.Eto;
using System.Diagnostics;

using ElegyApplication = Elegy.AppTemplate.Application;
using ElegyConsole = Elegy.ConsoleSystem.API.Console;

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
		private LaunchConfig mLaunchConfig;

		private IView? mRenderView = null;

		private Stopwatch mStopwatch;
		private double GetSeconds() => (double)mStopwatch.ElapsedTicks / Stopwatch.Frequency;

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

		public MainForm( string[] args )
		{
			mStopwatch = Stopwatch.StartNew();

			Style = "dark";
			Title = "My Eto Form";
			MinimumSize = new( 1280, 960 );

			mStatusLabel = new()
			{
				Text = "FPS:",
				Style = "dark"
			};

			mSurface = new()
			{
				Size = new( 1280, 720 )
			};

			mTextArea = new()
			{
				Size = new( 1280, 120 ),
				ReadOnly = true,
				Style = "dark"
			};

			mLogger = new( mTextArea );

			mLaunchConfig = new()
			{
				Args = args,
				EngineConfigName = "engineGuiConfig.json",
				ConsoleFrontends = [mLogger],
				WithMainWindow = false
			};

			Content = new StackLayout
			{
				Padding = 5,
				Items =
				{
					mStatusLabel,
					mSurface,
					new Label()
					{
						Style = "dark",
						Text = "Engine output:"
					},
					mTextArea
				},
				Size = new( -1, -1 )
			};

			ToolBar = new ToolBar
			{
				Style = "dark",
				Items =
				{
					CreateButton( "Restart engine", (sender, e) =>
					{
						if ( EngineSystem.IsRunning )
						{
							EngineSystem.Shutdown( "" );
						}

						if ( !ElegyApplication.Init( mLaunchConfig, null ) )
						{
							MessageBox.Show( EngineSystem.ShutdownReason ?? "unknown error" );
							return;
						}

						PlatformSystem.API.Platform.AddWindow( mSurface );
						mRenderView = Render.Instance.CreateView( mSurface );

						Plugins.RegisterPlugin( new FormApp() );
					} ),

					CreateButton( "Shutdown engine", (sender, e) =>
					{
						if ( EngineSystem.IsRunning )
						{
							EngineSystem.Shutdown( "" );
						}
					} ),

					CreateButton( "Free some memory", (sender, e) =>
					{
						GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive );
					} )
				}
			};

			mSurface.LoadComplete += OnLoad;
		}

		private void OnLoad( object? sender, EventArgs e )
		{
			PlatformSystem.API.Platform.AddWindow( mSurface );

			if ( !ElegyApplication.Init( mLaunchConfig, null ) )
			{
				MessageBox.Show( EngineSystem.ShutdownReason ?? "unknown error" );
				return;
			}

			// Register any convars here
			ElegyConsole.InitAssemblyConvars( typeof( MainForm ).Assembly );

			mRenderView = Render.Instance.GetView( mSurface );

			// Just to give the engine something to do
			Plugins.RegisterPlugin( new FormApp() );

			mLastTime = GetSeconds();
			mSurface.Draw += OnRender; // Pretty important!
		}

		private const double mRefreshRate = 165.0; // TODO: obtain screen refresh rate, mine's 165 Hz so I put it here
		private double mAverageFps = 60.0;
		private double mLastTime = 0.0;
		private double mVerticalSyncTimer = 1.0 / mRefreshRate;
		private int mFramerateUpdateCounter = 5;
		private void OnRender( object? sender, EventArgs e )
		{
			double deltaTime = GetSeconds() - mLastTime;

			mVerticalSyncTimer -= deltaTime;
			if ( mVerticalSyncTimer > 0.0 )
			{
				return;
			}
			mVerticalSyncTimer = 1.0 / mRefreshRate;

			double frameStart = GetSeconds();
			ElegyApplication.Update( 1.0f / (float)mRefreshRate );

			if ( mRenderView.RenderSize.X > 0.0f )
			{
				Render.RenderFrame( mRenderView );
			}
			else
			{
				mSurface.ForceResizeEvent();
				mRenderView.RenderSize = new( mSurface.RenderWidth, mSurface.RenderHeight );
			}

			mLastTime = GetSeconds();

			double fps = 1.0 / (mLastTime - frameStart);
			if ( fps > mAverageFps )
			{
				mAverageFps = fps;
			}
			else
			{
				mAverageFps = mAverageFps * 0.99 + fps * 0.01;
			}

			if ( mFramerateUpdateCounter-- == 0 )
			{
				var memoryInfo = GC.GetGCMemoryInfo();
				mStatusLabel.Text = string.Format( "Memory: {0,3} MB used, {1,3} MB reserved - FPS: {2:F}",
					memoryInfo.HeapSizeBytes / 1024 / 1024,
					memoryInfo.TotalCommittedBytes / 1024 / 1024,
					mAverageFps );
				mFramerateUpdateCounter = (int)mRefreshRate / 2;
			}
		}
	}

	public static class EntryPoint
	{
		private static void SetBackgroundColor<THandler>( float brightness )
			where THandler: class, Control.IHandler
		{
			Style.Add<THandler>( "dark", handler =>
			{
				handler.BackgroundColor = Color.FromGrayscale( brightness, 1.0f );
			} );
		}

		private static void SetDarkTheme()
		{
			SetBackgroundColor<Control.IHandler>( 0.15f );
			Style.Add<CommonControl.IHandler>( "dark", handler =>
			{
				handler.BackgroundColor = Color.FromGrayscale( 0.17f, 1.0f );
			} );
			Style.Add<TextControl.IHandler>( "dark", handler =>
			{
				handler.BackgroundColor = Color.FromGrayscale( 0.19f, 1.0f );
				handler.TextColor = Color.FromGrayscale( 0.94f, 1.0f );
			} );
		}

		public static void RunApplication( string[] args, string platform, Action also )
		{
			also();

			//if ( platform == Platforms.Wpf )
			//{
			//	SetDarkTheme();
			//}

			SilkWindow.Add( new EtoWindowPlatform() );
			SilkInput.Add( new EtoInputPlatform() );

			new Application( platform )
				.Run( new MainForm( args ) );
		}
	}
}
