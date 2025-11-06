// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SystemParallel = System.Threading.Tasks.Parallel;

namespace Elegy.Common.Parallel;

/// <summary>
/// Parallelisation utilities.
/// </summary>
/// <remarks>
/// <para>
/// Parallelisation in Elegy comes mainly in 4 flavours: 1) input data only, 2) job-local, 3) thread-local and 4) thread-and-job-local.
/// </para>
/// <para>
/// Common to all of them is the workflow of passing an input buffer, which gets subdivided according to a job size,
/// effectively getting divided and conquered by threads.
/// </para>
/// <para>
/// Job-local data is typically used to output the processed data into another buffer.
/// Thread-local data is used for other things, like per-thread memory pools.
/// </para>
/// </remarks>
public static unsafe class Parallel
{
	public delegate void Worker<TWorkItem>( BatchJob<TWorkItem> inputData, int workerIndex )
		where TWorkItem : unmanaged;

	public delegate void WorkerJobLocals<TWorkItem, TJobLocal>( BatchJob<TWorkItem, TJobLocal> inputData, int workerIndex )
		where TWorkItem : unmanaged;

	public delegate void WorkerThreadLocals<TWorkItem, TThreadLocal>( BatchJob<TWorkItem> inputData, int workerIndex, ref TThreadLocal threadLocalData )
		where TWorkItem : unmanaged;

	public delegate void WorkerThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal>( BatchJob<TWorkItem, TJobLocal> inputData, int workerIndex,
		ref TThreadLocal threadLocalData ) where TWorkItem : unmanaged;

	private class ThreadLocalWrapper<TWorkerFunc, TThreadLocal> : IDisposable
		where TThreadLocal : IDisposable
	{
		public ThreadLocalWrapper( TWorkerFunc body, TThreadLocal localData )
		{
			Action = body;
			ConcreteThreadData = localData;
		}

		public TWorkerFunc Action;
		public TThreadLocal ConcreteThreadData;

		public void Dispose()
		{
			ConcreteThreadData.Dispose();
		}
	}

	private static ParallelOptions CreateOptions( int numThreads )
		=> new() { MaxDegreeOfParallelism = numThreads };

	private static (int Start, int End, IntPtr Base) GetBatchJobBase<TWorkItem>( Span<TWorkItem> workItems, int jobSize, int i )
		where TWorkItem : unmanaged
		=> new()
		{
			Start = jobSize * i,
			End = Math.Min( workItems.Length - 1, ((i + 1) * jobSize) - 1 ),
			Base = (IntPtr)Unsafe.AsPointer( ref workItems[0] ),
		};

	/// <summary>
	/// Cuts up <paramref name="workItems"/> into <paramref name="jobSize"/> chunks.
	/// </summary>
	public static BatchJob<TWorkItem>[] GetJobs<TWorkItem>( Span<TWorkItem> workItems, int jobSize )
		where TWorkItem : unmanaged
	{
		int numItems = workItems.Length; // e.g. 1000
		int numJobs = ((numItems - 1) / jobSize) + 1; // e.g. 4 if jobSize is 256

		BatchJob<TWorkItem>[] jobs = new BatchJob<TWorkItem>[numJobs];
		for ( int i = 0; i < jobs.Length; i++ )
		{
			var jobInfo = GetBatchJobBase( workItems, jobSize, i );
			jobs[i] = new()
			{
				Start = jobInfo.Start,
				End = jobInfo.End,
				Base = (TWorkItem*)jobInfo.Base
			};
		}

		return jobs;
	}

	/// <summary>
	/// Cuts up <paramref name="workItems"/> into <paramref name="jobSize"/> chunks.
	/// Version with job locals.
	/// </summary>
	public static BatchJob<TWorkItem, TJobLocal>[] GetJobs<TWorkItem, TJobLocal>( Span<TWorkItem> workItems, int jobSize, Func<int, TJobLocal> jobLocalFactory )
		where TWorkItem : unmanaged
	{
		int numItems = workItems.Length; // e.g. 1000
		int numJobs = ((numItems - 1) / jobSize) + 1; // e.g. 4 if jobSize is 256

		BatchJob<TWorkItem, TJobLocal>[] jobs = new BatchJob<TWorkItem, TJobLocal>[numJobs];
		for ( int i = 0; i < jobs.Length; i++ )
		{
			var jobInfo = GetBatchJobBase( workItems, jobSize, i );
			jobs[i] = new()
			{
				Start = jobInfo.Start,
				End = jobInfo.End,
				Base = (TWorkItem*)jobInfo.Base,
				Local = jobLocalFactory( i )
			};
		}

		return jobs;
	}

	/// <summary>
	/// Cuts up <paramref name="workItems"/> into <paramref name="jobSize"/>
	/// chunks and processes them with <paramref name="numThreads"/> threads.
	/// </summary>
	public static void DivideAndDispatch<TWorkItem>( Span<TWorkItem> workItems, Worker<TWorkItem> worker, int numThreads, int jobSize )
		where TWorkItem : unmanaged
		=> DispatchJobs( GetJobs( workItems, jobSize ), worker, numThreads );

	/// <summary>
	/// Cuts up <paramref name="workItems"/> into <paramref name="jobSize"/>
	/// chunks and processes them with <paramref name="numThreads"/> threads.
	///
	/// Version with job-local data.
	/// </summary>
	public static void DivideAndDispatch<TWorkItem, TJobLocal>( Span<TWorkItem> workItems, WorkerJobLocals<TWorkItem, TJobLocal> worker,
		Func<int, TJobLocal> jobLocalFactory, int numThreads, int jobSize )
		where TWorkItem : unmanaged
		=> DispatchJobs( GetJobs( workItems, jobSize, jobLocalFactory ), worker, numThreads );

	/// <summary>
	/// Cuts up <paramref name="workItems"/> into <paramref name="jobSize"/>
	/// chunks and processes them with <paramref name="numThreads"/> threads.
	///
	/// Version with thread-local data.
	/// </summary>
	public static void DivideAndDispatch<TWorkItem, TThreadLocal>( Span<TWorkItem> workItems, WorkerThreadLocals<TWorkItem, TThreadLocal> worker,
		Func<TThreadLocal> threadLocalFactory, int numThreads, int jobSize )
		where TWorkItem : unmanaged
		where TThreadLocal : IDisposable
		=> DispatchJobs( GetJobs( workItems, jobSize ), worker, threadLocalFactory, numThreads );

	/// <summary>
	/// Cuts up <paramref name="workItems"/> into <paramref name="jobSize"/>
	/// chunks and processes them with <paramref name="numThreads"/> threads.
	///
	/// Version with thread-and-job-local data.
	/// </summary>
	public static void DivideAndDispatch<TWorkItem, TJobLocal, TThreadLocal>( Span<TWorkItem> workItems,
		WorkerThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal> worker,
		Func<int, TJobLocal> jobLocalFactory, Func<TThreadLocal> threadLocalFactory, int numThreads, int jobSize )
		where TWorkItem : unmanaged
		where TThreadLocal : IDisposable
		=> DispatchJobs( GetJobs( workItems, jobSize, jobLocalFactory ), worker, threadLocalFactory, numThreads );

	/// <summary>
	/// Processes jobs with <paramref name="numThreads"/> threads.
	/// </summary>
	public static void DispatchJobs<TWorkItem>( IEnumerable<BatchJob<TWorkItem>> jobs, Worker<TWorkItem> worker, int numThreads )
		where TWorkItem : unmanaged
	{
		SystemParallel.ForEach( jobs, CreateOptions( numThreads ),
			() => worker,
			Worker_Plain,
			_ => { } );
	}

	/// <summary>
	/// Processes jobs with <paramref name="numThreads"/> threads, with job-local data.
	/// </summary>
	public static void DispatchJobs<TWorkItem, TJobLocal>( IEnumerable<BatchJob<TWorkItem, TJobLocal>> jobs, WorkerJobLocals<TWorkItem, TJobLocal> worker,
		int numThreads )
		where TWorkItem : unmanaged
	{
		SystemParallel.ForEach( jobs, CreateOptions( numThreads ),
			() => worker,
			Worker_JobLocals,
			_ => { } );
	}

	/// <summary>
	/// Processes jobs with <paramref name="numThreads"/> threads, with thread-local data.
	/// </summary>
	public static void DispatchJobs<TWorkItem, TThreadLocal>(
		IEnumerable<BatchJob<TWorkItem>> jobs,
		WorkerThreadLocals<TWorkItem, TThreadLocal> worker, Func<TThreadLocal> threadLocalFactory, int numThreads )
		where TWorkItem : unmanaged
		where TThreadLocal : IDisposable
	{
		SystemParallel.ForEach( jobs, CreateOptions( numThreads ),
			() => new ThreadLocalWrapper<WorkerThreadLocals<TWorkItem, TThreadLocal>, TThreadLocal>( worker,
				threadLocalFactory() ),
			Worker_ThreadLocals,
			localData => localData.Dispose() );
	}

	/// <summary>
	/// Processes jobs with <paramref name="numThreads"/> threads, with thread-and-job-local data.
	/// </summary>
	public static void DispatchJobs<TWorkItem, TJobLocal, TThreadLocal>(
		IEnumerable<BatchJob<TWorkItem, TJobLocal>> jobs,
		WorkerThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal> worker, Func<TThreadLocal> threadLocalFactory,
		int numThreads )
		where TWorkItem : unmanaged
		where TThreadLocal : IDisposable
	{
		SystemParallel.ForEach( jobs, CreateOptions( numThreads ),
			() => new ThreadLocalWrapper<WorkerThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal>, TThreadLocal>(
				worker, threadLocalFactory() ),
			Worker_ThreadAndJobLocals,
			localData => localData.Dispose() );
	}

	private static Worker<TWorkItem> Worker_Plain<TWorkItem>( BatchJob<TWorkItem> job, ParallelLoopState loopState, long workerIndex, Worker<TWorkItem> worker )
		where TWorkItem : unmanaged
	{
		worker( job, (int)workerIndex );
		return worker;
	}

	private static WorkerJobLocals<TWorkItem, TJobLocal>
		Worker_JobLocals<TWorkItem, TJobLocal>(
			BatchJob<TWorkItem, TJobLocal> job,
			ParallelLoopState loopState,
			long workerIndex,
			WorkerJobLocals<TWorkItem, TJobLocal> worker )
		where TWorkItem : unmanaged
	{
		worker( job, (int)workerIndex );
		return worker;
	}

	private static ThreadLocalWrapper<WorkerThreadLocals<TWorkItem, TThreadLocal>, TThreadLocal>
		Worker_ThreadLocals<TWorkItem, TThreadLocal>(
			BatchJob<TWorkItem> job,
			ParallelLoopState loopState,
			long workerIndex,
			ThreadLocalWrapper<WorkerThreadLocals<TWorkItem, TThreadLocal>, TThreadLocal> threadLocalWrapper )
		where TWorkItem : unmanaged
		where TThreadLocal : IDisposable
	{
		threadLocalWrapper.Action( job, (int)workerIndex, ref threadLocalWrapper.ConcreteThreadData );
		return threadLocalWrapper;
	}

	private static ThreadLocalWrapper<WorkerThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal>, TThreadLocal>
		Worker_ThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal>(
			BatchJob<TWorkItem, TJobLocal> job,
			ParallelLoopState loopState,
			long workerIndex,
			ThreadLocalWrapper<WorkerThreadAndJobLocals<TWorkItem, TJobLocal, TThreadLocal>, TThreadLocal>
				threadLocalWrapper )
		where TWorkItem : unmanaged
		where TThreadLocal : IDisposable
	{
		threadLocalWrapper.Action( job, (int)workerIndex, ref threadLocalWrapper.ConcreteThreadData );
		return threadLocalWrapper;
	}
}
