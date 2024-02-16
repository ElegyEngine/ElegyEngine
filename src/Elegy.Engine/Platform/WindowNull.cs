// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Elegy
{
	/// <summary>
	/// Dummy window class for headless instances.
	/// </summary>
	public class WindowNull : IWindow
	{
		public IWindowHost? Parent => null;

		public IMonitor? Monitor { get; set; }
		public bool IsClosing { get; set; }

		public Rectangle<int> BorderSize => new Rectangle<int>();

		public bool IsVisible { get; set; }
		public Vector2D<int> Position { get; set; }
		public Vector2D<int> Size { get; set; }
		public string Title { get; set; }
		public WindowState WindowState { get; set; }
		public WindowBorder WindowBorder { get; set; }

		public bool TransparentFramebuffer => false;

		public bool TopMost { get; set; }

		public IGLContext? SharedContext => null;

		public string? WindowClass => null;

		public nint Handle => 0;

		public double Time => 0.0;

		public Vector2D<int> FramebufferSize => default( Vector2D<int> );

		public bool IsInitialized => false;

		public bool ShouldSwapAutomatically { get; set; }
		public bool IsEventDriven { get; set; }
		public bool IsContextControlDisabled { get; set; }
		public double FramesPerSecond { get; set; }
		public double UpdatesPerSecond { get; set; }

		public GraphicsAPI API => GraphicsAPI.None;

		public bool VSync { get; set; }

		public VideoMode VideoMode => VideoMode.Default;

		public int? PreferredDepthBufferBits => null;

		public int? PreferredStencilBufferBits => null;

		public Vector4D<int>? PreferredBitDepth => null;

		public int? Samples => null;

		public IGLContext? GLContext => null;

		public IVkSurface? VkSurface => null;

		public INativeWindow? Native => null;

		bool IView.IsClosing => false;

		Vector2D<int> IViewProperties.Size => new( 0, 0 );

		public event Action<Vector2D<int>>? Move;
		public event Action<WindowState>? StateChanged;
		public event Action<string[]>? FileDrop;
		public event Action<Vector2D<int>>? Resize;
		public event Action<Vector2D<int>>? FramebufferResize;
		public event Action? Closing;
		public event Action<bool>? FocusChanged;
		public event Action? Load;
		public event Action<double>? Update;
		public event Action<double>? Render;

		public void Close()
		{
		}

		public void ContinueEvents()
		{
		}

		public IWindow CreateWindow( WindowOptions opts )
		{
			return new WindowNull();
		}

		public void Dispose()
		{
		}

		public void DoEvents()
		{
		}

		public void DoRender()
		{
		}

		public void DoUpdate()
		{
		}

		public void Initialize()
		{
		}

		public object Invoke( Delegate d, params object[] args )
		{
			return null;
		}

		public Vector2D<int> PointToClient( Vector2D<int> point )
		{
			return point;
		}

		public Vector2D<int> PointToFramebuffer( Vector2D<int> point )
		{
			return point;
		}

		public Vector2D<int> PointToScreen( Vector2D<int> point )
		{
			return point;
		}

		public void Reset()
		{
		}

		public void Run( Action onFrame )
		{
		}

		public void SetWindowIcon( ReadOnlySpan<RawImage> icons )
		{
		}
	}
}
