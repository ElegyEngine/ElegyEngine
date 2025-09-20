// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Input;

namespace Elegy.Avalonia.Embedded;

/// <summary>
/// An implementation of <see cref="IKeyboardNavigationHandler"/> that does NOT listen for tab.
/// Focus navigation is done using the built-in Elegy actions instead, inside <see cref="AvaloniaControl"/>.
/// </summary>
internal sealed class ElegyKeyboardNavigationHandler : IKeyboardNavigationHandler
{
	private readonly KeyboardNavigationHandler mKeyboardNavigationHandler = new();

	void IKeyboardNavigationHandler.SetOwner( IInputRoot owner )
	{
	}

	public void Move( IInputElement element, NavigationDirection direction, KeyModifiers keyModifiers = KeyModifiers.None )
		=> mKeyboardNavigationHandler.Move( element, direction, keyModifiers );
}
