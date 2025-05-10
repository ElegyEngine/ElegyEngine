using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform.Storage;

namespace Elegy.Avalonia;

/// <summary>Implementation of <see cref="IStorageProviderFactory"/> for Elegy.</summary>
internal sealed class ElegyStorageProviderFactory : IStorageProviderFactory
{
	public IStorageProvider CreateProvider( TopLevel topLevel )
		=> new ElegyStorageProvider();
}
