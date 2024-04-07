// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Engine.API
{
	public static partial class FileSystem
	{
		internal static void SetFileSystem( FileSystemInternal? fileSystem )
			=> mFileSystem = fileSystem;
		

		private static FileSystemInternal? mFileSystem;
	}
}
