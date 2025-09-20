// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Input;
using Elegy.Avalonia.Embedded;

// TODO: Move this into Elegy.Avalonia.Embedded.Extensions
namespace Elegy.Avalonia.Extensions;

/// <summary>Contains extensions methods for <see cref="AppBuilder"/> related to Elegy.</summary>
public static class AppBuilderExtensions
{
	public static AppBuilder EmbedIntoElegy( this AppBuilder builder )
		=> builder
			.UseStandardRuntimePlatformSubsystem()
			.UseSkia()
			.UseWindowingSubsystem( () => ElegyPlatform.Initialize(), "Elegy Avalonia Platform" )
			.AfterSetup( _ =>
				AvaloniaLocator.CurrentMutable
					.Bind<IKeyboardNavigationHandler>()
					.ToTransient<ElegyKeyboardNavigationHandler>()
			);
}
