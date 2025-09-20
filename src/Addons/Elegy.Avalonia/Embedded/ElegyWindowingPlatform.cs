// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Platform;

namespace Elegy.Avalonia.Embedded;

internal sealed class ElegyWindowingPlatform : IWindowingPlatform
{
	public IWindowImpl CreateWindow()
		=> throw CreateNotImplementedException();

	public IWindowImpl CreateEmbeddableWindow()
		=> throw CreateNotImplementedException();

	public ITopLevelImpl CreateEmbeddableTopLevel()
		=> throw CreateNotImplementedException();

	private static NotImplementedException CreateNotImplementedException()
		=> new( "Sub windows aren't implemented yet" );

	public ITrayIconImpl? CreateTrayIcon()
		=> null;
}
