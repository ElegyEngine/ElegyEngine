
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Input.Raw;
using Avalonia.Platform;
using Elegy.Avalonia.Input;
using AvCompositor = Avalonia.Rendering.Composition.Compositor;
using AvKey = Avalonia.Input.Key;
using GdCursorShape = Silk.NET.Input.StandardCursor;
using ElMouseButton = Silk.NET.Input.MouseButton;
using Vector = Avalonia.Vector;

namespace Elegy.Avalonia;

/// <summary>Implementation of Avalonia <see cref="ITopLevelImpl"/> that renders to a Elegy texture.</summary>
internal sealed class ElegyTopLevelImpl : ITopLevelImpl
{
	private readonly ElegyPlatformGraphics mPlatformGraphics;
	private readonly IClipboard _clipboard;
	private readonly TouchDevice _touchDevice = new();

	private ElegySkiaSurface? _surface;
	private WindowTransparencyLevel _transparencyLevel = WindowTransparencyLevel.Transparent;
	private PixelSize _renderSize;
	private IInputRoot? _inputRoot;
	private GdCursorShape _cursorShape;
	private bool _isDisposed;
	private int _lastMouseDeviceId = ElegyDevices.EmulatedDeviceId;

	public double RenderScaling { get; private set; } = 1.0;

	double ITopLevelImpl.DesktopScaling
		=> 1.0;

	IPlatformHandle? ITopLevelImpl.Handle
		=> null;

	public AvCompositor Compositor { get; }

	public Size ClientSize { get; private set; }

	public WindowTransparencyLevel TransparencyLevel
	{
		get => _transparencyLevel;
		private set
		{
			if ( _transparencyLevel.Equals( value ) )
				return;

			_transparencyLevel = value;
			TransparencyLevelChanged?.Invoke( value );
		}
	}

	public Action<Rect>? Paint { get; set; }

	public Action<Size, WindowResizeReason>? Resized { get; set; }

	public Action? Closed { get; set; }

	public Action<RawInputEventArgs>? Input { get; set; }

	public Action? LostFocus { get; set; }

	public Action<GdCursorShape>? CursorChanged { get; set; }

	public Action<double>? ScalingChanged { get; set; }

	public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }

	IEnumerable<object> ITopLevelImpl.Surfaces
		=> GetOrCreateSurfaces();

	AcrylicPlatformCompensationLevels ITopLevelImpl.AcrylicCompensationLevels
		=> new( 1.0, 1.0, 1.0 );

	public ElegyTopLevelImpl( ElegyPlatformGraphics platformGraphics, IClipboard clipboard, AvCompositor compositor )
	{
		mPlatformGraphics = platformGraphics;
		_clipboard = clipboard;
		Compositor = compositor;

		platformGraphics.AddRef();
	}

	private ElegySkiaSurface CreateSurface()
	{
		if ( _isDisposed )
			throw new ObjectDisposedException( nameof( ElegyTopLevelImpl ) );

		return mPlatformGraphics.GetSharedContext().CreateSurface( _renderSize, RenderScaling );
	}

	public ElegySkiaSurface? TryGetSurface()
		=> _surface;

	public ElegySkiaSurface GetOrCreateSurface()
		=> _surface ??= CreateSurface();

	private IEnumerable<object> GetOrCreateSurfaces()
		=> new object[] { GetOrCreateSurface() };

	[SuppressMessage( "ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Doesn't affect correctness" )]
	public void SetRenderSize( PixelSize renderSize, double renderScaling )
	{
		var hasScalingChanged = RenderScaling != renderScaling;
		if ( _renderSize == renderSize && !hasScalingChanged )
			return;

		var oldClientSize = ClientSize;
		var unclampedClientSize = renderSize.ToSize( renderScaling );

		ClientSize = new Size( Math.Max( unclampedClientSize.Width, 0.0 ), Math.Max( unclampedClientSize.Height, 0.0 ) );
		RenderScaling = renderScaling;

		if ( _renderSize != renderSize )
		{
			_renderSize = renderSize;

			if ( _surface is not null )
			{
				_surface.Dispose();
				_surface = null;
			}

			if ( _isDisposed )
				return;

			_surface = CreateSurface();
		}

		if ( hasScalingChanged )
		{
			if ( _surface != null )
				_surface.RenderScaling = RenderScaling;
			ScalingChanged?.Invoke( RenderScaling );
		}

		if ( oldClientSize != ClientSize )
			Resized?.Invoke( ClientSize, hasScalingChanged ? WindowResizeReason.DpiChange : WindowResizeReason.Unspecified );
	}

	public void OnDraw( Rect rect )
		=> Paint?.Invoke( rect );

	/*
	public bool OnMouseMotion( InputEventMouseMotion inputEvent, ulong timestamp )
	{
		_lastMouseDeviceId = inputEvent.Device;

		if ( _inputRoot is null || Input is not { } input )
			return false;

		var args = new RawPointerEventArgs(
			ElegyDevices.GetMouse( inputEvent.Device ),
			timestamp,
			_inputRoot,
			RawPointerEventType.Move,
			CreateRawPointerPoint( inputEvent.Position, inputEvent.Pressure, inputEvent.Tilt ),
			inputEvent.GetRawInputModifiers()
		);

		input( args );

		return args.Handled;
	}

	public bool OnMouseButton( InputEventMouseButton inputEvent, ulong timestamp )
	{
		_lastMouseDeviceId = inputEvent.Device;

		if ( _inputRoot is null || Input is not { } input )
			return false;

		RawPointerEventArgs CreateButtonArgs( RawPointerEventType type )
			=> new(
				ElegyDevices.GetMouse( inputEvent.Device ),
				timestamp,
				_inputRoot,
				type,
				inputEvent.Position.ToAvaloniaPoint() / RenderScaling,
				inputEvent.GetRawInputModifiers()
			);

		RawMouseWheelEventArgs CreateWheelArgs( Vector delta )
			=> new(
				ElegyDevices.GetMouse( inputEvent.Device ),
				timestamp,
				_inputRoot,
				inputEvent.Position.ToAvaloniaPoint() / RenderScaling,
				delta,
				inputEvent.GetRawInputModifiers()
			);

		var args = (inputEvent.ButtonIndex, inputEvent.Pressed) switch
		{
			(ElMouseButton.Left, true)      => CreateButtonArgs( RawPointerEventType.LeftButtonDown ),
			(ElMouseButton.Left, false)     => CreateButtonArgs( RawPointerEventType.LeftButtonUp ),
			(ElMouseButton.Right, true)     => CreateButtonArgs( RawPointerEventType.RightButtonDown ),
			(ElMouseButton.Right, false)    => CreateButtonArgs( RawPointerEventType.RightButtonUp ),
			(ElMouseButton.Middle, true)    => CreateButtonArgs( RawPointerEventType.MiddleButtonDown ),
			(ElMouseButton.Middle, false)   => CreateButtonArgs( RawPointerEventType.MiddleButtonUp ),
			(ElMouseButton.Button4, true)  => CreateButtonArgs( RawPointerEventType.XButton1Down ),
			(ElMouseButton.Button4, false) => CreateButtonArgs( RawPointerEventType.XButton1Up ),
			(ElMouseButton.Button5, true)  => CreateButtonArgs( RawPointerEventType.XButton2Down ),
			(ElMouseButton.Button5, false) => CreateButtonArgs( RawPointerEventType.XButton2Up ),
			// TODO: mouse wheel
			//(ElMouseButton.WheelUp, _)      => CreateWheelArgs( new Vector( 0.0, inputEvent.Factor ) ),
			//(ElMouseButton.WheelDown, _)    => CreateWheelArgs( new Vector( 0.0, -inputEvent.Factor ) ),
			//(ElMouseButton.WheelLeft, _)    => CreateWheelArgs( new Vector( inputEvent.Factor, 0.0 ) ),
			//(ElMouseButton.WheelRight, _)   => CreateWheelArgs( new Vector( -inputEvent.Factor, 0.0 ) ),
			_                               => null
		};

		if ( args is null )
			return false;

		input( args );

		return args.Handled;
	}

	public bool OnScreenTouch( InputEventScreenTouch inputEvent, ulong timestamp )
	{
		if ( _inputRoot is null || Input is not { } input )
			return false;

		var args = new RawTouchEventArgs(
			_touchDevice,
			timestamp,
			_inputRoot,
			inputEvent.Pressed ? RawPointerEventType.TouchBegin : RawPointerEventType.TouchEnd,
			inputEvent.Position.ToAvaloniaPoint() / RenderScaling,
			InputModifiersProvider.GetRawInputModifiers(),
			inputEvent.Index
		);

		input( args );

		return args.Handled;
	}

	public bool OnScreenDrag( InputEventScreenDrag inputEvent, ulong timestamp )
	{
		if ( _inputRoot is null || Input is not { } input )
			return false;

		var args = new RawTouchEventArgs(
			_touchDevice,
			timestamp,
			_inputRoot,
			RawPointerEventType.TouchUpdate,
			CreateRawPointerPoint( inputEvent.Position, inputEvent.Pressure, inputEvent.Tilt ),
			inputEvent.GetRawInputModifiers(),
			inputEvent.Index
		);

		input( args );

		return args.Handled;
	}*/
	
	private RawPointerPoint CreateRawPointerPoint( Vector2 position, float pressure, Vector2 tilt )
		=> new()
		{
			Position = position.ToAvaloniaPoint() / RenderScaling,
			Twist = 0.0f,
			Pressure = pressure,
			XTilt = tilt.X * 90.0f,
			YTilt = tilt.Y * 90.0f
		};
	
	/*
	public bool OnKey( InputEventKey inputEvent, ulong timestamp )
	{
		if ( _inputRoot is null || Input is not { } input )
			return false;

		var keyCode = inputEvent.Keycode;
		var pressed = inputEvent.Pressed;
		var key = keyCode.ToAvaloniaKey();

		if ( key != AvKey.None )
		{
			var args = new RawKeyEventArgs(
				ElegyDevices.Keyboard,
				timestamp,
				_inputRoot,
				pressed ? RawKeyEventType.KeyDown : RawKeyEventType.KeyUp,
				key,
				inputEvent.GetRawInputModifiers(),
				inputEvent.PhysicalKeycode.ToAvaloniaPhysicalKey(),
				OS.GetKeycodeString( inputEvent.KeyLabel )
			);

			input( args );

			if ( args.Handled )
				return true;
		}

		// TODO: pressed && IsKeycodeUnicode( keyCode )
		if ( pressed )
		{
			var text = Char.ConvertFromUtf32( (int)inputEvent.Unicode );
			var args = new RawTextInputEventArgs( ElegyDevices.Keyboard, timestamp, _inputRoot, text );

			input( args );

			if ( args.Handled )
				return true;
		}

		return false;
	}
	*/

	// TODO: support joypads
	
	/*
	public bool OnJoypadButton( InputEventJoypadButton inputEvent, ulong timestamp )
	{
		if ( _inputRoot is null || Input is not { } input )
			return false;

		var args = new RawJoypadButtonEventArgs(
			ElegyDevices.GetJoypad( inputEvent.Device ),
			timestamp,
			_inputRoot,
			inputEvent.IsPressed() ? RawJoypadButtonEventType.ButtonDown : RawJoypadButtonEventType.ButtonUp,
			inputEvent.ButtonIndex
		);

		input( args );

		return args.Handled;
	}

	public bool OnJoypadMotion( InputEventJoypadMotion inputEvent, ulong timestamp )
	{
		if ( _inputRoot is null || Input is not { } input )
			return false;

		var args = new RawJoypadAxisEventArgs(
			ElegyDevices.GetJoypad( inputEvent.Device ),
			timestamp,
			_inputRoot,
			inputEvent.Axis,
			inputEvent.AxisValue
		);

		input( args );

		return args.Handled;
	}
	*/

	public void OnLostFocus()
		=> LostFocus?.Invoke();

	public bool OnMouseExited( ulong timestamp )
	{
		if ( _inputRoot is null || Input is not { } input )
			return false;

		var args = new RawPointerEventArgs(
			ElegyDevices.GetMouse( _lastMouseDeviceId ),
			timestamp,
			_inputRoot,
			RawPointerEventType.LeaveWindow,
			new Point( -1, -1 ),
			InputModifiersProvider.GetRawInputModifiers()
		);

		input( args );

		return args.Handled;
	}

	void ITopLevelImpl.SetInputRoot( IInputRoot inputRoot )
		=> _inputRoot = inputRoot;

	Point ITopLevelImpl.PointToClient( PixelPoint point )
		=> point.ToPoint( RenderScaling );

	PixelPoint ITopLevelImpl.PointToScreen( Point point )
		=> PixelPoint.FromPoint( point, RenderScaling );

	void ITopLevelImpl.SetCursor( ICursorImpl? cursor )
	{
		var cursorShape = (cursor as ElegyStandardCursorImpl)?.CursorShape ?? GdCursorShape.Arrow;
		if ( _cursorShape == cursorShape )
			return;

		_cursorShape = cursorShape;
		CursorChanged?.Invoke( cursorShape );
	}

	IPopupImpl? ITopLevelImpl.CreatePopup()
		=> null;

	void ITopLevelImpl.SetTransparencyLevelHint( IReadOnlyList<WindowTransparencyLevel> transparencyLevels )
	{
		foreach ( var transparencyLevel in transparencyLevels )
		{
			if ( transparencyLevel == WindowTransparencyLevel.Transparent || transparencyLevel == WindowTransparencyLevel.None )
			{
				TransparencyLevel = transparencyLevel;
				return;
			}
		}
	}

	void ITopLevelImpl.SetFrameThemeVariant( PlatformThemeVariant themeVariant )
	{
	}

	object? IOptionalFeatureProvider.TryGetFeature( Type featureType )
	{
		if ( featureType == typeof( IClipboard ) )
			return _clipboard;

		return null;
	}

	public void Dispose()
	{
		if ( _isDisposed )
			return;

		_isDisposed = true;

		if ( _surface is not null )
		{
			_surface.Dispose();
			_surface = null;
		}

		Closed?.Invoke();

		mPlatformGraphics.Release();
	}
}
