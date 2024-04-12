
# Integrating Elegy into Eto.Forms

Elegy uses Silk.NET for windowing & input abstraction. Silk offers SDL2 and GLFW as windowing backends, or as it calls them, windowing platforms, however that might not be 100% enough for some folks. You might want to render an engine view into, say, a panel in a native UI window.

This is basically the glue between Eto.Forms and Silk.NET.

Silk offers an excellent abstract, platform-independent windowing API. Interfaces like `IWindow`, `IInputContext` and so on. The idea is to implement an Eto control that implements `IWindow`, to the extent that it can:
* handle inputs - requires us to implement `IInputContext`
* be used to render stuff into, either via Veldrid or something else

Silk provides a Veldrid extension which already saves quite a bit of time:
```cs
public static GraphicsDevice CreateGraphicsDevice(this IView window, GraphicsDeviceOptions options)
```

`IWindow` is a type of `IView`, so we don't need to implement `IView` as a separate sort of control.

Internally it calls this:
```cs
public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(GraphicsDeviceOptions options, IView window)
```

And this code is executed:
```cs
var scDesc = new SwapchainDescription
(
    GetSwapchainSource(window.Native),
    (uint) window.Size.X,
    (uint) window.Size.Y,
    options.SwapchainDepthFormat,
    options.SyncToVerticalBlank,
    colorSrgb
);
var gd = GraphicsDevice.CreateVulkan(options, scDesc);
```

`window.Native` is an `INativeWindow`. It offers data about the underlying native window:
```cs
public interface INativeWindow
{
	(nint Display, nuint Window)? X11 { get; }
	(nint Display, nint Surface)? Wayland { get; }
	(nint Hwnd, nint HDC, nint HInstance)? Win32 { get; }
	(nint Window, nint Surface)? Android { get; }
	...
}
```
Of course, we can only implement the ones supported by Eto.

Windows can be created by `IWindowHost` i.e. `IWindowPlatform`. However, with Eto, it is better to first create the "Silk windows" on the form, and then register them in the engine. I think that's the way to go about it, instead of creating these windows inside the engine, not knowing anything about the UI.

With the SDL backend, keep in mind, arbitrary window creation is perfectly fine, because that's the intended usecase.

## What to implement (MVP)
Concretely, the following should be implemented:
* `IWindow`
	* `INativeWindow`
	* event handlers for resizing, drawing etc.
* `IInputContext`
	* `IKeyboard`
	* `IMouse`
	* etc.
* `IWindowPlatform`
	* rather easy since there's no window creation per se, can just put `throw new UnsupportedException()` in a lot of spots
