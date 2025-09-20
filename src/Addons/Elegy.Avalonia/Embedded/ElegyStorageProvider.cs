// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Avalonia.Platform.Storage;
using Environment = System.Environment;

namespace Elegy.Avalonia.Embedded;

/// <summary>Implementation of <see cref="IStorageProvider"/> for Elegy.</summary>
internal sealed class ElegyStorageProvider : IStorageProvider
{
	public bool CanOpen
		=> true;

	public bool CanSave
		=> true;

	public bool CanPickFolder
		=> true;

	// TODO: Elegy.Avalonia: do we need file browsers here?
	public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync( FilePickerOpenOptions options )
		=> throw new NotImplementedException();

	public async Task<IStorageFile?> SaveFilePickerAsync( FilePickerSaveOptions options )
		=> throw new NotImplementedException();

	public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync( FolderPickerOpenOptions options )
		=> throw new NotImplementedException();

	public Task<IStorageBookmarkFile?> OpenFileBookmarkAsync( string bookmark )
	{
		var fileInfo = new FileInfo( bookmark );
		var storageFile = fileInfo.Exists ? new BclStorageFile( fileInfo ) : null;
		return Task.FromResult<IStorageBookmarkFile?>( storageFile );
	}

	public Task<IStorageBookmarkFolder?> OpenFolderBookmarkAsync( string bookmark )
	{
		var folderInfo = new DirectoryInfo( bookmark );
		var storageFolder = folderInfo.Exists ? new BclStorageFolder( folderInfo ) : null;
		return Task.FromResult<IStorageBookmarkFolder?>( storageFolder );
	}

	public Task<IStorageFile?> TryGetFileFromPathAsync( Uri filePath )
	{
		if ( filePath.IsAbsoluteUri )
		{
			var fileInfo = new FileInfo( filePath.LocalPath );
			if ( fileInfo.Exists )
				return Task.FromResult<IStorageFile?>( new BclStorageFile( fileInfo ) );
		}

		return Task.FromResult<IStorageFile?>( null );
	}

	public Task<IStorageFolder?> TryGetFolderFromPathAsync( Uri folderPath )
	{
		if ( folderPath.IsAbsoluteUri )
		{
			var folderInfo = new DirectoryInfo( folderPath.LocalPath );
			if ( folderInfo.Exists )
				return Task.FromResult<IStorageFolder?>( new BclStorageFolder( folderInfo ) );
		}

		return Task.FromResult<IStorageFolder?>( null );
	}

	public Task<IStorageFolder?> TryGetWellKnownFolderAsync( WellKnownFolder wellKnownFolder )
	{
		var path = wellKnownFolder switch
		{
			WellKnownFolder.Desktop   => Environment.GetFolderPath( Environment.SpecialFolder.Desktop ),
			WellKnownFolder.Documents => Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),
			WellKnownFolder.Music     => Environment.GetFolderPath( Environment.SpecialFolder.MyMusic ),
			WellKnownFolder.Pictures  => Environment.GetFolderPath( Environment.SpecialFolder.MyPictures ),
			WellKnownFolder.Videos    => Environment.GetFolderPath( Environment.SpecialFolder.MyVideos ),
			_                         => null
		};

		var storageFolder = path is null ? null : new BclStorageFolder( new DirectoryInfo( path ) );
		return Task.FromResult<IStorageFolder?>( storageFolder );
	}
}
