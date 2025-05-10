using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Avalonia.Threading;
using GdDispatcher = Elegy.Dispatcher;
using SysTimer = System.Threading.Timer;

namespace Elegy.Avalonia;

/// <summary>An implementation of <see cref="IDispatcherImpl"/> that uses the underlying Elegy dispatcher.</summary>
[SuppressMessage(
	"Design",
	"CA1001:Types that own disposable fields should be disposable",
	Justification = "This type has equivalent to a static lifetime"
)]
internal sealed class ElegyDispatcherImpl : IDispatcherImpl
{
	private readonly Thread mMainThread;
	private readonly SysTimer mTimer;
	private readonly SendOrPostCallback mInvokeSignaled; // cached delegate
	private readonly SendOrPostCallback mInvokeTimer;    // cached delegate

	private readonly Stopwatch mStopwatch = Stopwatch.StartNew();

	public long Now => mStopwatch.ElapsedMilliseconds;

	public long NowMicroseconds => mStopwatch.Elapsed.Microseconds;

	public bool CurrentThreadIsLoopThread => mMainThread == Thread.CurrentThread;

	public event Action? Signaled;

	public event Action? Timer;

	public ElegyDispatcherImpl( Thread mainThread )
	{
		mMainThread = mainThread;
		mInvokeSignaled = InvokeSignaled;
		mInvokeTimer = InvokeTimer;
		mTimer = new( OnTimerTick, this, Timeout.Infinite, Timeout.Infinite );
	}

	public void UpdateTimer( long? dueTimeInMs )
	{
		var interval = dueTimeInMs is { } value
			? Math.Clamp( value - Now, 0L, 0xFFFFFFFEL )
			: Timeout.Infinite;

		mTimer.Change( interval, Timeout.Infinite );
	}

	private void OnTimerTick( object? state )
		//	=> GdDispatcher.SynchronizationContext.Post( mInvokeTimer, null );
		=> SynchronizationContext.Current.Post( mInvokeTimer, null );

	public void Signal()
		//	=> GdDispatcher.SynchronizationContext.Post( mInvokeSignaled, this );
		=> SynchronizationContext.Current.Post( mInvokeSignaled, this );

	private void InvokeSignaled( object? state )
		=> Signaled?.Invoke();

	private void InvokeTimer( object? state )
		=> Timer?.Invoke();
}
