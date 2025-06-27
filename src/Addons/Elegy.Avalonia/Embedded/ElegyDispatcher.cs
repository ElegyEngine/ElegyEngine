
using System.Collections.Concurrent;

namespace Elegy
{
	public sealed class ElegySynchronizationContext : SynchronizationContext, IDisposable
	{
		private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> mQueue = new();

		public override void Send( SendOrPostCallback d, object? state )
		{
			if ( Current == this )
			{
				d( state );
			}
			else
			{
				TaskCompletionSource source = new();
				mQueue.Add( (st =>
				{
					try
					{
						d( st );
					}
					finally
					{
						source.SetResult();
					}
				}, state) );
				source.Task.Wait();
			}
		}

		public override void Post( SendOrPostCallback d, object state ) => mQueue.Add( (d, state) );

		/// <summary>
		/// Calls the Key method on each workItem object in the _queue to activate their callbacks.
		/// </summary>
		public void ExecutePendingContinuations()
		{
			(SendOrPostCallback Callback, object State) tuple;
			while ( mQueue.TryTake( out tuple ) )
				tuple.Callback( tuple.State );
		}

		public void Dispose() => mQueue.Dispose();
	}
}


namespace Elegy
{
	/// <summary>
	/// ElegyTaskScheduler contains a linked list of tasks to perform as a queue. Methods
	/// within the class are used to control the queue and perform the contained tasks.
	/// </summary>
	public sealed class ElegyTaskScheduler : TaskScheduler, IDisposable
	{
		/// <summary>The queue of tasks for the task scheduler.</summary>
		private readonly LinkedList<Task> mTasks = new();

		/// <summary>The current synchronization context.</summary>
		internal ElegySynchronizationContext Context { get; }

		/// <summary>Constructs a new ElegyTaskScheduler instance.</summary>
		public ElegyTaskScheduler()
		{
			Context = new ElegySynchronizationContext();
			SynchronizationContext.SetSynchronizationContext( Context );
		}

		protected override void QueueTask( Task task )
		{
			lock ( mTasks )
				mTasks.AddLast( task );
		}

		protected override bool TryExecuteTaskInline( Task task, bool taskWasPreviouslyQueued )
		{
			if ( SynchronizationContext.Current != Context )
				return false;
			if ( taskWasPreviouslyQueued )
				TryDequeue( task );
			return TryExecuteTask( task );
		}

		protected override bool TryDequeue( Task task )
		{
			lock ( mTasks )
				return mTasks.Remove( task );
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			lock ( mTasks )
			{
				foreach ( Task task in mTasks )
					yield return task;
			}
		}

		/// <summary>
		/// Executes all queued tasks and pending tasks from the current context.
		/// </summary>
		public void Activate()
		{
			ExecuteQueuedTasks();
			Context.ExecutePendingContinuations();
		}

		/// <summary>
		/// Loops through and attempts to execute each task in _tasks.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException"></exception>
		private void ExecuteQueuedTasks()
		{
			Task? task;
			do
			{
				lock ( mTasks )
				{
					if ( !mTasks.Any() )
						return;
					task = mTasks.First?.Value;
					mTasks.RemoveFirst();
				}
			} while ( task == null || TryExecuteTask( task ) );

			throw new InvalidOperationException();
		}

		public void Dispose() => Context.Dispose();
	}
}

namespace Elegy
{
	public static class Dispatcher
	{
		internal static ElegyTaskScheduler? DefaultElegyTaskScheduler;

		internal static void InitializeDefaultElegyTaskScheduler()
		{
			DefaultElegyTaskScheduler?.Dispose();
			DefaultElegyTaskScheduler = new ElegyTaskScheduler();
		}

		public static ElegySynchronizationContext SynchronizationContext => DefaultElegyTaskScheduler.Context;
	}
}
