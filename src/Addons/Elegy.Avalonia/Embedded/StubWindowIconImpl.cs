using Avalonia.Platform;

namespace Elegy.Avalonia.Embedded;

/// <summary>A fake window icon that can't be displayed but can still be saved.</summary>
internal sealed class StubWindowIconImpl : IWindowIconImpl
{
	private readonly MemoryStream mStream;

	public StubWindowIconImpl( MemoryStream stream )
		=> mStream = stream;

	public void Save( Stream outputStream )
	{
		mStream.Position = 0L;
		mStream.CopyTo( outputStream );
	}
}
