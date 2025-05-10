using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;

namespace Elegy.Avalonia;

internal sealed class ElegyCursorFactory : ICursorFactory
{
	public ICursorImpl GetCursor( StandardCursorType cursorType )
		=> new ElegyStandardCursorImpl( cursorType.ToElegyCursorShape() );

	public ICursorImpl CreateCursor( IBitmapImpl cursor, PixelPoint hotSpot )
		=> throw new NotSupportedException( "Custom cursors aren't supported" );
}
