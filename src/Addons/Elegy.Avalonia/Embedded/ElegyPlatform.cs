using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Platform;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Elegy.Avalonia.Input;
using AvCompositor = Avalonia.Rendering.Composition.Compositor;

namespace Elegy.Avalonia.Embedded;

/// <summary>Contains Elegy to Avalonia platform initialization.</summary>
internal static class ElegyPlatform
{
	private static AvCompositor? mCompositor;
	private static ManualRenderTimer? mRenderTimer;
	private static ElegyDispatcherImpl? mDispatcherImpl;

	public static AvCompositor Compositor
		=> mCompositor ?? throw new InvalidOperationException( $"{nameof( ElegyPlatform )} hasn't been initialized" );

	public static void Initialize()
	{
		AvaloniaSynchronizationContext.AutoInstall = false; // Elegy has its own sync context, don't replace it

		var platformGraphics = new ElegyPlatformGraphics();
		var renderTimer = new ManualRenderTimer();
		mDispatcherImpl = new ElegyDispatcherImpl( Thread.CurrentThread );

		AvaloniaLocator.CurrentMutable
			.Bind<IClipboard>().ToConstant( new ElegyClipboard() )
			.Bind<ICursorFactory>().ToConstant( new ElegyCursorFactory() )
			.Bind<IDispatcherImpl>().ToConstant( mDispatcherImpl )
			.Bind<IKeyboardDevice>().ToConstant( ElegyDevices.Keyboard )
			.Bind<IPlatformGraphics>().ToConstant( platformGraphics )
			.Bind<IPlatformIconLoader>().ToConstant( new StubPlatformIconLoader() )
			.Bind<IPlatformSettings>().ToConstant( new ElegyPlatformSettings() )
			.Bind<IRenderTimer>().ToConstant( renderTimer )
			.Bind<IWindowingPlatform>().ToConstant( new ElegyWindowingPlatform() )
			.Bind<IStorageProviderFactory>().ToConstant( new ElegyStorageProviderFactory() )
			.Bind<PlatformHotkeyConfiguration>().ToConstant( CreatePlatformHotKeyConfiguration() )
			.Bind<ManagedFileDialogOptions>().ToConstant( new ManagedFileDialogOptions { AllowDirectorySelection = true } );

		mRenderTimer = renderTimer;
		mCompositor = new AvCompositor( platformGraphics );
	}

	private static PlatformHotkeyConfiguration CreatePlatformHotKeyConfiguration()
		=> OperatingSystem.IsMacOS()
			? new PlatformHotkeyConfiguration( commandModifiers: KeyModifiers.Meta, wholeWordTextActionModifiers: KeyModifiers.Alt )
			: new PlatformHotkeyConfiguration( commandModifiers: KeyModifiers.Control );

	public static void TriggerRenderTick()
	{
		Debug.Assert( mRenderTimer is not null );
		Debug.Assert( mDispatcherImpl is not null );
		
		mRenderTimer.TriggerTick( new TimeSpan( mDispatcherImpl.NowMicroseconds * 10L ) );
	}
}
