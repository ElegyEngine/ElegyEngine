using System.Collections.Concurrent;
using Avalonia.Input;

namespace Elegy.Avalonia.Input;

/// <summary>Contains the various Avalonia devices created from Elegy.</summary>
public static class ElegyDevices
{
	private static readonly ConcurrentDictionary<int, IMouseDevice> MouseById = new();

	/// <summary>The device identifier used by emulated devices.</summary>
	public const int EmulatedDeviceId = -1;

	/// <summary>Gets the keyboard device.</summary>
	/// <remarks>At this time, we don't support multiple keyboard devices since Avalonia needs a single one for focus management.</remarks>
	public static IKeyboardDevice Keyboard { get; } = new KeyboardDevice();

	/// <summary>Gets a mouse device for a given Elegy device identifier.</summary>
	/// <param name="deviceId">The device identifier.</param>
	/// <returns>A mouse device.</returns>
	public static IMouseDevice GetMouse( int deviceId )
		=> MouseById.GetOrAdd( deviceId, static id => new MouseDevice( new Pointer( id, PointerType.Mouse, id == 0 ) ) );
}
