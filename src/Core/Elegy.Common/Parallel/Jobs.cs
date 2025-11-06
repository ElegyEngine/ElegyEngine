// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Parallel;

public unsafe struct BatchJob<TWorkItem> where TWorkItem : unmanaged
{
	public int Start;
	public int End;
	public TWorkItem* Base;

	public Span<T> OutputSpan<T>( T* basePtr ) where T : unmanaged => new( basePtr + Start, Length );
	public Span<T> OutputSpan<T>( IntPtr basePtr ) where T : unmanaged => new( (T*)basePtr + Start, Length );
	public Span<TWorkItem> DataSpan => new( Base + Start, Length );
	public int Length => End - Start + 1;
}

public unsafe struct BatchJob<TWorkItem, TJobLocal> where TWorkItem : unmanaged
{
	public int Start;
	public int End;
	public TWorkItem* Base;
	public TJobLocal Local;

	public Span<T> OutputSpan<T>( T* basePtr ) where T : unmanaged => new( basePtr + Start, Length );
	public Span<TWorkItem> DataSpan => new( Base + Start, Length );
	public int Length => End - Start + 1;
}
