
using Eto.Forms;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;

using SilkWindowState = Silk.NET.Windowing.WindowState;
using EtoWindowState = Eto.Forms.WindowState;

namespace Elegy.GuiLauncher
{
	public partial class SilkWindow : Control, IWindow
	{
		public IMonitor Monitor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsClosing
		{
			get => false;
			set { }
		}

		public Rectangle<int> BorderSize => throw new NotImplementedException();

		public bool IsVisible
		{
			get => Visible;
			set => Visible = value;
		}

		public Vector2D<int> Position
		{
			get => new Vector2D<int>( Location.X, Location.Y );
			set { }
		}
		
		public string Title
		{
			get => ParentWindow.Title;
			set => ParentWindow.Title = value;
		}
		
		public SilkWindowState WindowState
		{
			get => ParentWindow.WindowState switch
			{
				EtoWindowState.Normal => SilkWindowState.Normal,
				EtoWindowState.Maximized => SilkWindowState.Maximized,
				EtoWindowState.Minimized => SilkWindowState.Minimized,
				_ => throw new NotSupportedException()
			};

			set => ParentWindow.WindowState = value switch
			{ 
				SilkWindowState.Normal => EtoWindowState.Normal,
				SilkWindowState.Fullscreen => EtoWindowState.Maximized,
				SilkWindowState.Maximized => EtoWindowState.Maximized,
				SilkWindowState.Minimized => EtoWindowState.Minimized,
				_ => throw new NotSupportedException()
			};
		}

		public WindowBorder WindowBorder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool TransparentFramebuffer => throw new NotImplementedException();

		public bool TopMost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public IGLContext SharedContext => throw new NotImplementedException();

		public string WindowClass => throw new NotImplementedException();

		public IntPtr Handle => throw new NotImplementedException();

		public double Time => throw new NotImplementedException();

		public Vector2D<int> FramebufferSize => throw new NotImplementedException();

		public bool IsInitialized => throw new NotImplementedException();

		public bool ShouldSwapAutomatically { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsEventDriven { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool IsContextControlDisabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public double FramesPerSecond { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public double UpdatesPerSecond { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public GraphicsAPI API => throw new NotImplementedException();

		public bool VSync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public VideoMode VideoMode => throw new NotImplementedException();

		public int? PreferredDepthBufferBits => throw new NotImplementedException();

		public int? PreferredStencilBufferBits => throw new NotImplementedException();

		public Vector4D<int>? PreferredBitDepth => throw new NotImplementedException();

		public int? Samples => throw new NotImplementedException();

		public IGLContext GLContext => throw new NotSupportedException();

		public IVkSurface VkSurface => throw new NotImplementedException();

		public INativeWindow Native => throw new NotImplementedException();

		IWindowHost IWindow.Parent { get; }

		Vector2D<int> IWindowProperties.Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		Vector2D<int> IViewProperties.Size => throw new NotImplementedException();

		bool IView.IsClosing => throw new NotImplementedException();

		public event Action<Vector2D<int>> Move;
		public event Action<SilkWindowState> StateChanged;
		public event Action<string[]> FileDrop;
		public event Action<Vector2D<int>> Resize;
		public event Action<Vector2D<int>> FramebufferResize;
		public event Action Closing;
		public event Action<bool> FocusChanged;
		public event Action<double> Update;
		public event Action<double> Render;

		event Action IView.Load
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		public void Close()
		{
			throw new NotImplementedException();
		}

		public void ContinueEvents()
		{
			throw new NotImplementedException();
		}

		public IWindow CreateWindow( WindowOptions opts )
		{
			throw new NotImplementedException();
		}

		public void DoEvents()
		{
			throw new NotImplementedException();
		}

		public void DoRender()
		{
			throw new NotImplementedException();
		}

		public void DoUpdate()
		{
			throw new NotImplementedException();
		}

		public object Invoke( Delegate d, params object[] args )
		{
			throw new NotImplementedException();
		}

		public Vector2D<int> PointToClient( Vector2D<int> point )
		{
			throw new NotImplementedException();
		}

		public Vector2D<int> PointToFramebuffer( Vector2D<int> point )
		{
			throw new NotImplementedException();
		}

		public Vector2D<int> PointToScreen( Vector2D<int> point )
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void Run( Action onFrame )
		{
			throw new NotImplementedException();
		}

		public void SetWindowIcon( ReadOnlySpan<RawImage> icons )
		{
			throw new NotImplementedException();
		}

		void IView.Initialize()
		{
			throw new NotImplementedException();
		}
	}
}
