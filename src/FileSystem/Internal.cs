// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static partial class FileSystem
	{
		internal static void SetFileSystem( FileSystemInternal? fileSystem )
			=> mFileSystem = fileSystem;
		

		private static FileSystemInternal? mFileSystem;
	}
}
