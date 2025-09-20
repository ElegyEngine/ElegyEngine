// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia;
using Avalonia.Input.Platform;
using Elegy.Avalonia.Platform;
using SilkWindow = Silk.NET.Windowing.Window;
using SilkInput = Silk.NET.Input.InputWindowExtensions;

namespace Elegy.Avalonia;

// NOTE: "Embedded" is all the code for embedding Avalonia into Elegy,
// while "Platform" is all the code for embedding Elegy into an Avalonia window.
// It should also provide passthru so it can be both nested and embedded at the same time

// TODO: Elegy.Avalonia: Implement top-level manager

/// <summary>
/// Handles Avalonia integration with Elegy.
/// </summary>
public static class AvaloniaManager
{
	private static Embedded.ElegyTopLevelImpl? mTopLevelImpl;

	/// <summary>
	/// Initialises Avalonia as an in-game UI library. 
	/// </summary>
	public static void InitEmbedded()
	{
		var locator = AvaloniaLocator.Current;

		// Passthrough mode, where Elegy (with embedded Avalonia) is itself embedded into an Avalonia window
		bool passthrough = SilkWindow.GetWindowPlatform( viewOnly: false ) is AvaloniaWindowPlatform;
		Embedded.ElegyPlatform.Initialize( passthrough );

		mTopLevelImpl = new( Embedded.ElegyPlatform.Graphics, locator.GetService<IClipboard>()!, Embedded.ElegyPlatform.Compositor );
	}

	/// <summary>
	/// Initialises Avalonia as a platform backend to be used by Elegy's platform system.
	/// </summary>
	/// <remarks>
	/// Assumes there is already an Avalonia window up and running with the Vulkan Skia backend.
	/// Call this before starting Elegy's app framework.
	/// </remarks>
	public static void InitPlatformBackend()
	{
		// TODO: Elegy.Avalonia platform mode
		SilkWindow.Add( new AvaloniaWindowPlatform() );
		SilkInput.Add( new AvaloniaInputPlatform() );
	}
}

/*
/// <summary>Renders an Avalonia control and forwards input to it.</summary>
public class AvaloniaControl : GdControl {

	private AvControl? _control;
	private double _renderScaling = 1.0;
	private ElegyTopLevel? _topLevel;

	/// <summary>Gets or sets the underlying Avalonia control that will be rendered.</summary>
	public AvControl? Control {
		get => _control;
		set {
			if (ReferenceEquals(_control, value))
				return;

			_control = value;

			if (_topLevel is not null)
				_topLevel.Content = _control;
		}
	}

	/// <summary>Gets or sets the render scaling for the Avalonia control. Defaults to 1.0.</summary>
	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Doesn't affect correctness")]
	public double RenderScaling {
		get => _renderScaling;
		set {
			if (_renderScaling == value)
				return;

			_renderScaling = value;
			OnResized();
			QueueRedraw();
		}
	}

	/// <summary>
	/// Gets or sets whether some Elegy UI actions will be automatically mapped to an <see cref="InputElement.KeyDownEvent"/> event.
	/// The mapped actions are ui_left, ui_right, ui_up, ui_down, ui_accept and ui_cancel.
	/// Defaults to true.
	/// </summary>
	public bool AutoConvertUIActionToKeyDown { get; set; } = true;

	/// <summary>Gets the underlying Avalonia top-level element.</summary>
	/// <returns>The Avalonia top-level element.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the control isn't ready or has been disposed.</exception>
	public ElegyTopLevel GetTopLevel()
		=> _topLevel ?? throw new InvalidOperationException($"The {nameof(AvaloniaControl)} isn't initialized");

	/// <summary>Gets the underlying Elegy texture where <see cref="Control"/> is rendered.</summary>
	/// <returns>A texture.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the control isn't ready or has been disposed.</exception>
	public Texture2D GetTexture()
		=> GetTopLevel().Impl.GetOrCreateSurface().GdTexture;

	protected override bool InvokeElegyClassMethod(in Elegy_string_name method, NativeVariantPtrArgs args, out Elegy_variant ret) {
		if (method == Node.MethodName._Ready && args.Count == 0) {
			_Ready();
			ret = default;
			return true;
		}

		if (method == Node.MethodName._Process && args.Count == 1) {
			_Process(VariantUtils.ConvertTo<double>(args[0]));
			ret = default;
			return true;
		}

		if (method == CanvasItem.MethodName._Draw && args.Count == 0) {
			_Draw();
			ret = default;
			return true;
		}

		if (method == MethodName._GuiInput && args.Count == 1) {
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(args[0]));
			ret = default;
			return true;
		}

		if (method == MethodName._HasPoint && args.Count == 1) {
			ret = VariantUtils.CreateFrom(_HasPoint(VariantUtils.ConvertTo<Vector2>(args[0])));
			return true;
		}

		return base.InvokeElegyClassMethod(method, args, out ret);
	}

	protected override bool HasElegyClassMethod(in Elegy_string_name method)
		=> method == Node.MethodName._Ready
			|| method == Node.MethodName._Process
			|| method == CanvasItem.MethodName._Draw
			|| method == MethodName._GuiInput
			|| method == MethodName._HasPoint
			|| base.HasElegyClassMethod(method);

	public override void _Ready() {
		if (Engine.IsEditorHint())
			return;

		// Skia outputs a premultiplied alpha image, ensure we got the correct blend mode if the user didn't specify any
		Material ??= new CanvasItemMaterial {
			BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha,
			LightMode = CanvasItemMaterial.LightModeEnum.Unshaded
		};

		var locator = AvaloniaLocator.Current;

		if (locator.GetService<IPlatformGraphics>() is not ElegyVkPlatformGraphics graphics) {
			GD.PrintErr("No Elegy platform graphics found, did you forget to register your Avalonia app with UseElegy()?");
			return;
		}

		var topLevelImpl = new ElegyTopLevelImpl(graphics, locator.GetRequiredService<IClipboard>(), ElegyPlatform.Compositor) {
			CursorChanged = OnAvaloniaCursorChanged
		};

		topLevelImpl.SetRenderSize(GetFrameSize(), RenderScaling);

		_topLevel = new ElegyTopLevel(topLevelImpl) {
			Background = null,
			Content = Control,
			TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent, WindowTransparencyLevel.None }
		};

		_topLevel.Prepare();
		_topLevel.StartRendering();

		Resized += OnResized;
		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;
		MouseExited += OnMouseExited;

		if (HasFocus())
			OnFocusEntered();
	}

	public override void _Process(double delta) {
		ElegyPlatform.TriggerRenderTick();

		// We might have cleared the texture after resize to prevent corruption on AMD GPU (see ElegySkiaGpuRenderSession),
		// force a re-render.
		if (_topLevel?.Impl.TryGetSurface()?.DrawCount <= 2)
			RenderAvalonia();
	}

	private PixelSize GetFrameSize()
		=> PixelSize.FromSize(Size.ToAvaloniaSize(), 1.0);

	private void RenderAvalonia()
		=> _topLevel!.Impl.OnDraw(new Rect(Size.ToAvaloniaSize()));

	private void OnAvaloniaCursorChanged(CursorShape cursor)
		=> MouseDefaultCursorShape = cursor;

	private void OnResized() {
		if (_topLevel is null)
			return;

		_topLevel.Impl.SetRenderSize(GetFrameSize(), RenderScaling);
		RenderAvalonia();
	}

	private void OnFocusEntered() {
		if (_topLevel is null)
			return;

		_topLevel.Focus();

		if (KeyboardNavigationHandler.GetNext(_topLevel, NavigationDirection.Next) is not { } inputElement)
			return;

		NavigationMethod navigationMethod;

		if (GdInput.IsActionPressed(ElegyBuiltInActions.UIFocusNext) || GdInput.IsActionPressed(ElegyBuiltInActions.UIFocusPrev))
			navigationMethod = NavigationMethod.Tab;
		else if (GdInput.GetMouseButtonMask() != 0)
			navigationMethod = NavigationMethod.Pointer;
		else
			navigationMethod = NavigationMethod.Unspecified;

		inputElement.Focus(navigationMethod);
	}

	private void OnFocusExited()
		=> _topLevel?.Impl.OnLostFocus();

	public override void _Draw() {
		if (_topLevel is null)
			return;


		var surface = _topLevel.Impl.GetOrCreateSurface();

		DrawTexture(surface.GdTexture, Vector2.Zero);
	}

	public override void _GuiInput(InputEvent @event) {
		if (_topLevel is null)
			return;

		if (TryHandleInput(_topLevel.Impl, @event) || TryHandleAction(@event))
			AcceptEvent();
	}

	private bool TryHandleAction(InputEvent inputEvent) {
		if (!inputEvent.IsActionType())
			return false;

		if (inputEvent.IsActionPressed(ElegyBuiltInActions.UIFocusNext, true, true))
			return TryMoveFocus(NavigationDirection.Next, inputEvent);

		if (inputEvent.IsActionPressed(ElegyBuiltInActions.UIFocusPrev, true, true))
			return TryMoveFocus(NavigationDirection.Previous, inputEvent);

		if (AutoConvertUIActionToKeyDown) {

			if (inputEvent.IsActionPressed(ElegyBuiltInActions.UILeft, true, true))
				return SimulateKeyDownFromAction(inputEvent, GdKey.Left);

			if (inputEvent.IsActionPressed(ElegyBuiltInActions.UIRight, true, true))
				return SimulateKeyDownFromAction(inputEvent, GdKey.Right);

			if (inputEvent.IsActionPressed(ElegyBuiltInActions.UIUp, true, true))
				return SimulateKeyDownFromAction(inputEvent, GdKey.Up);

			if (inputEvent.IsActionPressed(ElegyBuiltInActions.UIDown, true, true))
				return SimulateKeyDownFromAction(inputEvent, GdKey.Down);

			if (inputEvent.IsActionPressed(ElegyBuiltInActions.UIAccept, true, true))
				return SimulateKeyDownFromAction(inputEvent, GdKey.Enter);

			if (inputEvent.IsActionPressed(ElegyBuiltInActions.UICancel, true, true))
				return SimulateKeyDownFromAction(inputEvent, GdKey.Escape);

		}

		return false;
	}

	private bool SimulateKeyDownFromAction(InputEvent inputEvent, GdKey key) {
		// if the action already matches the key we're going to simulate, abort: it already got through TryHandleInput and wasn't handled
		if (inputEvent is InputEventKey inputEventKey && inputEventKey.Keycode == key)
			return false;

		if (_topLevel?.FocusManager?.GetFocusedElement() is not { } currentElement)
			return false;

		var args = new KeyEventArgs {
			RoutedEvent = InputElement.KeyDownEvent,
			Key = key.ToAvaloniaKey(),
			KeyModifiers = inputEvent.GetKeyModifiers()
		};
		currentElement.RaiseEvent(args);
		return args.Handled;
	}

	private static bool TryHandleInput(ElegyTopLevelImpl impl, InputEvent inputEvent)
		=> inputEvent switch {
			InputEventMouseMotion mouseMotion => impl.OnMouseMotion(mouseMotion, Time.GetTicksMsec()),
			InputEventMouseButton mouseButton => impl.OnMouseButton(mouseButton, Time.GetTicksMsec()),
			InputEventScreenTouch screenTouch => impl.OnScreenTouch(screenTouch, Time.GetTicksMsec()),
			InputEventScreenDrag screenDrag => impl.OnScreenDrag(screenDrag, Time.GetTicksMsec()),
			InputEventKey key => impl.OnKey(key, Time.GetTicksMsec()),
			InputEventJoypadButton joypadButton => impl.OnJoypadButton(joypadButton, Time.GetTicksMsec()),
			InputEventJoypadMotion joypadMotion => impl.OnJoypadMotion(joypadMotion, Time.GetTicksMsec()),
			_ => false
		};

	private bool TryMoveFocus(NavigationDirection direction, InputEvent inputEvent) {
		if (_topLevel?.FocusManager is not { } focusManager)
			return false;

		var currentElement = focusManager.GetFocusedElement() ?? _topLevel;

		// ElegyTopLevel has a Continue tab navigation since we want to be able to focus the Elegy controls
		// once we're done with the Avalonia ones. However, if there's no Elegy control, we want to act as Cycle.
		var nextElement = GetNextTabElement(currentElement, direction);
		if (nextElement is null) {
			var nextGdControl = direction switch {
				NavigationDirection.Next => FindNextValidFocus(),
				NavigationDirection.Previous => FindPrevValidFocus(),
				_ => null
			};

			if ((nextGdControl is null || nextGdControl == this) && (object) currentElement != _topLevel)
				nextElement = GetNextTabElement(_topLevel, direction);
		}


		if (nextElement is null)
			return false;

		nextElement.Focus(NavigationMethod.Tab, inputEvent.GetKeyModifiers());
		return true;
	}

	private static IInputElement? GetNextTabElement(IInputElement element, NavigationDirection direction) {
		var previous = element;

		while (true) {
			// GetNext doesn't take IsEffectivelyEnabled into account, check it manually
			var next = KeyboardNavigationHandler.GetNext(previous, direction);
			if (next is null || next.IsEffectivelyEnabled)
				return next;

			// handle potential all-disabled cycle
			if (next == element)
				return null;

			previous = next;
		}
	}

	private void OnMouseExited()
		=> _topLevel?.Impl.OnMouseExited(Time.GetTicksMsec());

	public override bool _HasPoint(Vector2 point)
		=> _topLevel?.InputHitTest(point.ToAvaloniaPoint() / _topLevel.RenderScaling, false) is not null;

	protected override void Dispose(bool disposing) {
		if (disposing && _topLevel is not null) {

			Resized -= OnResized;
			FocusEntered -= OnFocusEntered;
			FocusExited -= OnFocusExited;
			MouseExited -= OnMouseExited;

			_topLevel.Dispose();
			_topLevel = null;
		}

		base.Dispose(disposing);
	}

}
*/
