using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Elegy.Common.Assets;
using ElegyView = Elegy.RenderSystem.Objects.View;
using ElegyTexture = Elegy.AssetSystem.Resources.Texture;
using ElegyRender = Elegy.RenderSystem.API.Render;

namespace Elegy.Avalonia.Platform;

public partial class ElegyViewport : Control
{
	protected override void OnKeyDown( KeyEventArgs e )
	{
		InputContext.OnKeyDown( e );
		base.OnKeyDown( e );
	}

	protected override void OnKeyUp( KeyEventArgs e )
	{
		InputContext.OnKeyUp( e );
		base.OnKeyUp( e );
	}

	protected override void OnPointerMoved( PointerEventArgs e )
	{
		// TODO: Remainder of the input stuff
		base.OnPointerMoved( e );
	}

	protected override void OnPointerPressed( PointerPressedEventArgs e )
	{
		base.OnPointerPressed( e );
	}

	protected override void OnPointerReleased( PointerReleasedEventArgs e )
	{
		base.OnPointerReleased( e );
	}
}
