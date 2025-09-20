// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces.Services;

namespace Elegy.Common;

public class DefaultFileSystem : IFileSystem
{
	public string? PathToFile( string path )
	{
		if ( File.Exists( path ) )
		{
			return path;
		}

		return null;
	}

	public string? PathToDirectory( string path )
	{
		if ( Directory.Exists( path ) )
		{
			return path;
		}

		return null;
	}
}
