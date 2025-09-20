// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia.Platform;

namespace Elegy.Avalonia.Embedded;

/// <summary>Elegy Vulkan-based <see cref="IPlatformGraphics"/> implementation.</summary>
internal sealed class ElegyPlatformGraphics : IPlatformGraphics, IDisposable
{
	private ElegySkiaGpu? mContext;
	private int mRefCount;

	bool IPlatformGraphics.UsesSharedContext
		=> true;

	public ElegySkiaGpu GetSharedContext()
	{
		if ( Volatile.Read( ref mRefCount ) == 0 )
			ThrowDisposed();

		if ( mContext is null || mContext.IsLost )
		{
			mContext?.Dispose();
			mContext = null;
			mContext = new ElegySkiaGpu();
		}

		return mContext;
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
		=> Interlocked.Increment( ref mRefCount );

	public void Release()
	{
		if ( Interlocked.Decrement( ref mRefCount ) == 0 )
			Dispose();
	}


	public void Dispose()
	{
		if ( mContext is not null )
		{
			mContext.Dispose();
			mContext = null;
		}
	}
}
