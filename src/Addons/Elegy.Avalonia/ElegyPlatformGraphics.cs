using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Platform;

namespace Elegy.Avalonia;

/// <summary>Elegy Vulkan-based <see cref="IPlatformGraphics"/> implementation.</summary>
internal sealed class ElegyPlatformGraphics : IPlatformGraphics, IDisposable
{
	private ElegySkiaGpu? _context;
	private int _refCount;

	bool IPlatformGraphics.UsesSharedContext
		=> true;

	public ElegySkiaGpu GetSharedContext()
	{
		if ( Volatile.Read( ref _refCount ) == 0 )
			ThrowDisposed();

		if ( _context is null || _context.IsLost )
		{
			_context?.Dispose();
			_context = null;
			_context = new ElegySkiaGpu();
		}

		return _context;
	}

	[DoesNotReturn]
	[MethodImpl( MethodImplOptions.NoInlining )]
	private static void ThrowDisposed()
		=> throw new ObjectDisposedException( nameof( ElegyPlatformGraphics ) );

	IPlatformGraphicsContext IPlatformGraphics.CreateContext()
		=> throw new NotSupportedException();

	IPlatformGraphicsContext IPlatformGraphics.GetSharedContext()
		=> GetSharedContext();

	public void AddRef()
		=> Interlocked.Increment( ref _refCount );

	public void Release()
	{
		if ( Interlocked.Decrement( ref _refCount ) == 0 )
			Dispose();
	}


	public void Dispose()
	{
		if ( _context is not null )
		{
			_context.Dispose();
			_context = null;
		}
	}
}
