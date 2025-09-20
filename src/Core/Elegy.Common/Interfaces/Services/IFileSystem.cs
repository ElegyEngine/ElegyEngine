// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Interfaces.Services;

/// <summary>
/// Elegy.FileSystem module decoupler. If you wish to use the
/// plugin system or some other engine module, without the
/// engine whatsoever, this is what allows it to do that.
///
/// It implements the bare minimum needed for the engine
/// modules to use it.
/// </summary>
public interface IFileSystem
{
	/// <summary>
	/// Refer to "Files.PathTo". (cannot link it here)
	/// </summary>
	string? PathToFile( string path );

	/// <summary>
	/// Refer to "Files.PathTo". (cannot link it here)
	/// </summary>
	string? PathToDirectory( string path );
}
