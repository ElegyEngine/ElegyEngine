// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Threading;
using Silk.NET.Input;

namespace Elegy.Avalonia.Platform;

/// <summary>
/// Elegy window for Avalonia UI. Provides input context.
/// Rendering is done through <see cref="ElegyViewport"/>.
/// </summary>
/// <remarks>
/// Basically provides an input context that is then populated by
/// child <see cref="ElegyViewport"/> instances.
/// </remarks>
public class ElegyWindow : Window
{
	private DispatcherTimer mTimer;

	public SilkWindowAdapter SilkWindowAdapter { get; protected set; }
	public AvaloniaInputContext InputContext { get; protected set; }

	public ElegyWindow()
	{
		mTimer = new( DispatcherPriority.BeforeRender );

		SilkWindowAdapter = new()
		{
			NativeWindow = this
		};
		SilkWindowAdapter.Initialize();
		InputContext = (AvaloniaInputContext)SilkWindowAdapter.CreateInput();

		mTimer.Tick += Update;
	}

	protected override void OnDetachedFromLogicalTree( LogicalTreeAttachmentEventArgs e )
	{
		SilkWindowAdapter.DoUpdate();
		base.OnDetachedFromLogicalTree( e );
	}

	public void Update( object? sender, EventArgs e )
	{
		SilkWindowAdapter.DoUpdate();
	}
}
