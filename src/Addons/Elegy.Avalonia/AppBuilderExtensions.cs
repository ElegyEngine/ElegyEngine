using Avalonia;
using Avalonia.Input;

namespace Elegy.Avalonia;

/// <summary>Contains extensions methods for <see cref="AppBuilder"/> related to Elegy.</summary>
public static class AppBuilderExtensions
{
	public static AppBuilder UseElegy( this AppBuilder builder )
		=> builder
			.UseStandardRuntimePlatformSubsystem()
			.UseSkia()
			.UseWindowingSubsystem( ElegyPlatform.Initialize )
			.AfterSetup( _ =>
				AvaloniaLocator.CurrentMutable
					.Bind<IKeyboardNavigationHandler>()
					.ToTransient<ElegyKeyboardNavigationHandler>()
			);
}
