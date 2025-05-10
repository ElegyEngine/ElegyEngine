using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Input;

namespace Elegy.Avalonia;

/// <summary>
/// A <see cref="TopLevel"/> used with Elegy.
/// This is implicitly created by <see cref="AvaloniaControl"/>.
/// </summary>
public sealed class ElegyTopLevel : EmbeddableControlRoot
{
	internal ElegyTopLevelImpl Impl { get; }

	static ElegyTopLevel()
		// TopLevel has Cycle navigation mode but we want the focus to be able to leave Avalonia to return back to Elegy: use Continue
		=> KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<ElegyTopLevel>( KeyboardNavigationMode.Continue );

	internal ElegyTopLevel( ElegyTopLevelImpl impl )
		: base( impl )
		=> Impl = impl;
}
