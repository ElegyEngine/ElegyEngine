using Elegy.Common.Interfaces.Services;
using Elegy.FileSystem.API;

namespace Elegy.FileSystem;

internal class FileSystemImpl : IFileSystem
{
	public string? PathToFile( string path )
		=> Files.PathTo( path, PathFlags.File );

	public string? PathToDirectory( string path )
		=> Files.PathTo( path, PathFlags.Directory );
}
