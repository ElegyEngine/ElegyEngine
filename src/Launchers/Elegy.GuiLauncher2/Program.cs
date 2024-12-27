using Avalonia;
using System;
using Avalonia.Vulkan;

namespace Elegy.GuiLauncher2;

class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main( string[] args )
		=> BuildAvaloniaApp()
			.StartWithClassicDesktopLifetime( args );

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			//.WithElegy() - this is currently done in App.OnFrameworkInitializationCompleted
			.With( new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.Vulkan] } )
			.With( new X11PlatformOptions { RenderingMode = [X11RenderingMode.Vulkan] } )
			.With( new VulkanOptions
			{
				VulkanInstanceCreationOptions = new()
				{
					#if DEBUG
					UseDebug = true
					#endif
				}
			} )
			//.WithInterFont()
			.LogToTrace();
}
