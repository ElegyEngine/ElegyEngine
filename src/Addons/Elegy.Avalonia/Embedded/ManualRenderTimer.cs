// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Rendering;

namespace Elegy.Avalonia.Embedded;

/// <summary>A <see cref="IRenderTimer"/> implementation that is only triggered manually.</summary>
internal sealed class ManualRenderTimer : IRenderTimer
{
	public event Action<TimeSpan>? Tick;

	bool IRenderTimer.RunsInBackground
		=> false;

	public void TriggerTick( TimeSpan elapsed )
		=> Tick?.Invoke( elapsed );
}
