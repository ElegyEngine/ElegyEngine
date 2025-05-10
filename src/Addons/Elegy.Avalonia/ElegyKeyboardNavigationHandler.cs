using Avalonia.Input;

namespace Elegy.Avalonia;

/// <summary>
/// An implementation of <see cref="IKeyboardNavigationHandler"/> that does NOT listen for tab.
/// Focus navigation is done using the built-in Elegy actions instead, inside <see cref="AvaloniaControl"/>.
/// </summary>
internal sealed class ElegyKeyboardNavigationHandler : IKeyboardNavigationHandler
{
	private readonly KeyboardNavigationHandler _keyboardNavigationHandler = new();

	void IKeyboardNavigationHandler.SetOwner( IInputRoot owner )
	{
	}

	public void Move( IInputElement element, NavigationDirection direction, KeyModifiers keyModifiers = KeyModifiers.None )
		=> _keyboardNavigationHandler.Move( element, direction, keyModifiers );
}
