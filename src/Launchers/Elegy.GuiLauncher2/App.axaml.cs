using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ElegyApp = Elegy.AppTemplate.Application;
using SilkWindow = Silk.NET.Windowing.Window;
using SilkInput = Silk.NET.Input.InputWindowExtensions;

namespace Elegy.GuiLauncher2;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load( this );
	}

	public override void OnFrameworkInitializationCompleted()
	{
		SilkWindow.Add( null );
		SilkInput.Add( null );
		
		ElegyApp.Init( new()
		{
			EngineConfigName = "engineGuiConfig.json",
			
			// Do not call Platform.CreateWindow, it will
			// be created externally as an Avalonia control
			WithMainWindow = false,
		}, null );
		
		if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
		{
			desktop.MainWindow = new MainWindow();
		}

		base.OnFrameworkInitializationCompleted();
	}
}
