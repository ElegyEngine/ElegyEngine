// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Silk.NET.Maths;
using Silk.NET.Windowing;
using AvaloniaNativeWindow = Avalonia.Controls.Window;

namespace Elegy.Avalonia.Platform;

// TODO: Elegy.Avalonia: implement adapters

/// <summary>
/// Adapts an Avalonia window to Silk's IWindow interface.
/// Important: rendering is done through <see cref="ElegyViewport"/>!
/// </summary>
public partial class SilkWindowAdapter
{
	public required AvaloniaNativeWindow NativeWindow { get; init; }

	public Vector2D<int> Size
	{
		get => new( (int)NativeWindow.Width, (int)NativeWindow.Height );
		set
		{
			NativeWindow.Width = value.X;
			NativeWindow.Height = value.Y;
		}
	}

	public string Title
	{
		get => NativeWindow.Title ?? "AvaloniaWindow";
		set => NativeWindow.Title = value;
	}

	public double FramesPerSecond { get; set; } = 60.0f;
	public double UpdatesPerSecond { get; set; } = 60.0f;
	public GraphicsAPI API => GraphicsAPI.DefaultVulkan;

	public bool VSync // TODO: obtain vsync info
	{
		get => false;
		set { }
	}

	public VideoMode VideoMode => VideoMode.Default; // TODO: obtain video mode

	public void Dispose()
	{
	}

	public void Initialize()
	{
		if ( IsInitialized )
		{
			return;
		}

		IsInitialized = true;
	}

	public void DoRender()
	{
		// Rendering is scheduled externally
	}

	public void DoUpdate()
	{
	}

	public void DoEvents()
	{
	}

	public void ContinueEvents()
	{
	}

	public void Reset()
	{
	}

	public void Focus()
	{
		NativeWindow.Focus();
	}

	public void Close()
	{
		NativeWindow.Close();
	}

	public void Run( Action onFrame )
	{
		// TODO: run an update frame before rendering, using Avalonia timers possibly
	}

	

	public Vector2D<int> FramebufferSize => Size;

	public bool IsInitialized { get; private set; }
}
