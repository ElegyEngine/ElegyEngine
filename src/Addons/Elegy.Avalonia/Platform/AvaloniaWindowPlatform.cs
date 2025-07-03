using Silk.NET.Windowing;

namespace Elegy.Avalonia.Platform;

// TODO: Avalonia input context for Silk

public class AvaloniaWindowPlatform : IWindowPlatform
{
	public IWindow CreateWindow( WindowOptions opts )
	{
		// TODO: create another window with a builtin ElegyViewport inside
		return null!;
	}

	public IView GetView( ViewOptions? opts = null )
	{
		return null!;
	}

	public void ClearContexts()
	{
	}

	public IEnumerable<IMonitor> GetMonitors()
		=> [];

	public IMonitor GetMainMonitor()
	{
		return null!;
	}

	public bool IsSourceOfView( IView view )
		=> view is SilkWindowAdapter;

	public bool IsViewOnly => true;
	public bool IsApplicable => true;
}
