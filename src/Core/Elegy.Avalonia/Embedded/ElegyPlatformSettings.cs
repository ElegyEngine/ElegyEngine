// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Platform;

namespace Elegy.Avalonia.Embedded;

/// <summary>Implementation of <see cref="IPlatformSettings"/> for Elegy.</summary>
internal sealed class ElegyPlatformSettings : DefaultPlatformSettings
{
	public override PlatformColorValues GetColorValues()
		=> new()
		{
			ThemeVariant = PlatformThemeVariant.Dark,
			ContrastPreference = ColorContrastPreference.NoPreference,
			// TODO: Elegy-Avalonia: possibly query the OS about the accent colour
			AccentColor1 = new( 255, 0, 120, 215 )
			//AccentColor1 = DisplayServer.GetAccentColor().ToAvaloniaColor()
		};
}
