using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;
using Elegy.Avalonia.Extensions;

namespace Elegy.Avalonia.Embedded;

internal sealed class ElegyCursorFactory : ICursorFactory
{
	public ICursorImpl GetCursor( StandardCursorType cursorType )
		=> new ElegyStandardCursorImpl( cursorType.ToElegyCursorShape() );

	public ICursorImpl CreateCursor( IBitmapImpl cursor, PixelPoint hotSpot )
		=> throw new NotSupportedException( "Custom cursors aren't supported" );
}
