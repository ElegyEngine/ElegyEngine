using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Elegy.Avalonia;
using Elegy.Common.Assets;
using ElegyApp = Elegy.App.AppTemplate;
using SilkWindow = Silk.NET.Windowing.Window;
using SilkInput = Silk.NET.Input.InputWindowExtensions;

namespace Elegy.GuiLauncher2;

public partial class App : Application
{
	private readonly LaunchConfig mConfig = new()
	{
		// Do not call Platform.CreateWindow, it will
		// be created externally as an Avalonia control
		WithMainWindow = false,
		EngineConfigName = "engineGuiConfig.json"
	};

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load( this );
	}

	public override void OnFrameworkInitializationCompleted()
	{
		AvaloniaManager.InitPlatformBackend();

		if ( ElegyApp.Init( mConfig, SilkWindow.GetWindowPlatform( false ) ) )
		{
			
		}

		if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
		{
			desktop.MainWindow = new MainWindow();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
