﻿using Avalonia.Platform;
using Silk.NET.Input;

namespace Elegy.Avalonia;

/// <summary>A standard cursor, represented by a <see cref="GdCursorShape"/> enum value.</summary>
internal sealed class ElegyStandardCursorImpl : ICursorImpl
{
	public StandardCursor CursorShape { get; }

	public ElegyStandardCursorImpl( StandardCursor cursorShape )
		=> CursorShape = cursorShape;

	public override string ToString()
		=> CursorShape.ToString();

	public void Dispose()
	{
	}
}
