// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Avalonia.Utilities;

/// <summary>A reusable <see cref="IDisposable"/> implementation that does nothing when disposed.</summary>
internal sealed class EmptyDisposable : IDisposable
{
	public static EmptyDisposable Instance { get; } = new();

	private EmptyDisposable()
	{
	}

	public void Dispose()
	{
	}
}
