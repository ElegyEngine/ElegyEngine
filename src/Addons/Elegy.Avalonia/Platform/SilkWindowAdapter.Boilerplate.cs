using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using AvaloniaWindowState = Avalonia.Controls.WindowState;

namespace Elegy.Avalonia.Platform;

public partial class SilkWindowAdapter : IWindow
{
	public bool ShouldSwapAutomatically { get; set; }
	public bool IsEventDriven { get; set; }
	public bool IsContextControlDisabled { get; set; }
	public bool IsVisible { get; set; }
	public Vector2D<int> Position { get; set; }

	Vector2D<int> IWindowProperties.Size
	{
		get => Size;
		set => Size = value;
	}

	public bool IsClosing { get; set; }

	public WindowState WindowState
	{
		get => NativeWindow.WindowState switch
		{
			AvaloniaWindowState.Minimized  => WindowState.Minimized,
			AvaloniaWindowState.Maximized  => WindowState.Maximized,
			AvaloniaWindowState.FullScreen => WindowState.Fullscreen,
			_                              => WindowState.Normal
		};
		set { }
	}

	public WindowBorder WindowBorder
	{
		get => WindowBorder.Fixed;
		set { }
	}

	public bool TopMost
	{
		get => NativeWindow.Topmost;
		set => NativeWindow.Topmost = value;
	}

	public IntPtr Handle => 0;
	public INativeWindow? Native => null;

	public bool TransparentFramebuffer => false;
	public IGLContext? SharedContext => null;
	public string? WindowClass => null;

	public int? PreferredDepthBufferBits => null;
	public int? PreferredStencilBufferBits => null;
	public Vector4D<int>? PreferredBitDepth => null;
	public int? Samples => null;

	public Rectangle<int> BorderSize => new( 0, 0, 0, 0 ); // TODO: obtain "border size"

	public IWindow CreateWindow( WindowOptions opts )
	{
		// TODO: create another window with a builtin ElegyViewport inside
		throw new NotImplementedException();
	}

	public Vector2D<int> PointToClient( Vector2D<int> point )
	{
		return point;
	}

	public Vector2D<int> PointToScreen( Vector2D<int> point )
	{
		return point;
	}

	public Vector2D<int> PointToFramebuffer( Vector2D<int> point )
	{
		return point;
	}

	public object Invoke( Delegate d, params object[] args )
	{
		return default!;
	}

	public void SetWindowIcon( ReadOnlySpan<RawImage> icons )
	{
		// Nothing
	}

	public event Action<Vector2D<int>>? Move;
	public event Action<WindowState>? StateChanged;
	public event Action<string[]>? FileDrop;

	public IGLContext? GLContext => null;
	public IVkSurface? VkSurface => null;

	public IWindowHost? Parent => null;

	public IMonitor? Monitor
	{
		get => null;
		set { }
	}

	public double Time => 0.0;

	bool IView.IsClosing => false;

	public event Action<Vector2D<int>>? Resize;
	public event Action<Vector2D<int>>? FramebufferResize;
	public event Action? Closing;
	public event Action<bool>? FocusChanged;
	public event Action? Load;
	public event Action<double>? Update;
	public event Action<double>? Render;
}
