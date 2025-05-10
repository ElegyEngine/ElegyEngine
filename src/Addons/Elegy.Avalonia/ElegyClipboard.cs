using Avalonia.Input;
using Avalonia.Input.Platform;

namespace Elegy.Avalonia;

/// <summary>An implementation of <see cref="IClipboard"/> that uses Elegy clipboard methods.</summary>
internal sealed class ElegyClipboard : IClipboard
{
	public Task<string?> GetTextAsync()
		//	=> Task.FromResult<string?>( PlatformSystem.Clipboard );
		=> Task.FromResult<string?>( string.Empty );

	public Task SetTextAsync( string? text )
	{
		//PlatformSystem.Clipboard = text ?? string.Empty;
		return Task.CompletedTask;
	}

	public Task ClearAsync()
		=> SetTextAsync( String.Empty );

	public Task SetDataObjectAsync( IDataObject data )
		=> Task.CompletedTask;

	public Task<string[]> GetFormatsAsync()
		=> Task.FromResult( Array.Empty<string>() );

	public Task<object?> GetDataAsync( string format )
		=> Task.FromResult<object?>( null );
}
