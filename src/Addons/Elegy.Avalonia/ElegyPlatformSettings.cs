using Avalonia.Platform;


namespace Elegy.Avalonia;

/// <summary>Implementation of <see cref="IPlatformSettings"/> for Elegy.</summary>
internal sealed class ElegyPlatformSettings : DefaultPlatformSettings
{
	public override PlatformColorValues GetColorValues()
		=> new()
		{
			ThemeVariant = PlatformThemeVariant.Dark,
			ContrastPreference = ColorContrastPreference.NoPreference,
			// TODO: possibly query the OS about the accent colour
			AccentColor1 = new( 255, 0, 120, 215 )
			//AccentColor1 = DisplayServer.GetAccentColor().ToAvaloniaColor()
		};
}
