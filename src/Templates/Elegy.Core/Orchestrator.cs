// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using Elegy.Common.Interfaces.Services;
using Elegy.Common.Utilities;

namespace Elegy.Core;

[DebuggerDisplay( "{DebuggerDisplay,nq}" )]
public readonly record struct OrchestratorEntry<T>( Enum Id, Func<T, bool>? Init, Action? Shutdown, Action? Other )
{
	public string Name => Enum.GetName( Id.GetType(), Id ) ?? "#INVALID#";
	public string FullName => $"{Id.GetType().Name}.{Name}";

	public override string ToString()
		=> (Init, Shutdown, Other) switch
		{
			(not null, null, null) => $"{FullName} - i:{Init.Method.Name}",
			(null, not null, null) => $"{FullName} - s:{Shutdown.Method.Name}",
			(null, null, not null) => $"{FullName} - o:{Other.Method.Name}",
			(not null, not null, null) => $"{FullName} - i:{Init.Method.Name} - s:{Shutdown.Method.Name}",
			_ => FullName
		};

	private string DebuggerDisplay => ToString();
}

/// <summary>
/// Engine startup orchestrator. Makes sure things execute in order and, in
/// case of failure, shut down in order from the last system that initialised.
/// </summary>
[DebuggerDisplay( "{Entries}" )]
public class Orchestrator<T>
{
	private ILogSystem mLog => ElegyInterfaceLocator.GetLogSystem();

	public List<OrchestratorEntry<T>> Entries { get; init; } = new();

	public bool Init( T config )
	{
		int lastIndex = 0;

		foreach ( var entry in Entries )
		{
			if ( entry.Init is not null )
			{
				mLog.Verbose( $"Entering stage {entry.FullName}..." );

				if ( !entry.Init( config ) )
				{
					mLog.Fatal( $"Stage '{entry.Name}' failed to initialise, look for error messages above" );
					break;
				}
			}

			entry.Other?.Invoke();

			lastIndex++;
		}

		bool failed = lastIndex < Entries.Count - 1;
		if ( failed )
		{
			for ( int i = lastIndex; i >= 0; i-- )
			{
				Entries[i].Shutdown?.Invoke();
			}
		}

		return !failed;
	}

	public void Shutdown()
	{
		for ( int i = Entries.Count - 1; i >= 0; i-- )
		{
			Entries[i].Shutdown?.Invoke();
		}
	}

	public bool Run( T config, Action? main = null )
	{
		if ( !Init( config ) )
		{
			return false;
		}

		main?.Invoke();

		Shutdown();
		return true;
	}

	/// <summary>
	/// Prints active entries to the console.
	/// </summary>
	public void Report()
	{
		foreach ( var entry in Entries )
		{
			if ( entry.Other is not null || entry.Init is not null )
			{
				Console.WriteLine( $"{entry.FullName} - {entry.Other?.ToString() ?? entry.Init!.ToString()}" );
			}
		}

		for ( int i = Entries.Count - 1; i >= 0; i-- )
		{
			var entry = Entries[i];
			if ( entry.Shutdown is not null )
			{
				Console.WriteLine( $"{entry.FullName} - {entry.Shutdown}" );
			}
		}
	}

	/// <summary>
	/// Removes an event of specified ID.
	/// </summary>
	public Orchestrator<T> Remove( Enum id )
	{
		Entries.RemoveAll( e => Equals( e.Id, id ) );
		return this;
	}

	/// <summary>
	/// Inserts a startup/shutdown event at the specified index.
	/// </summary>
	public Orchestrator<T> Insert( int index, Enum id, Func<T, bool>? stageInit = null, Action? stageShutdown = null )
	{
		// TODO: do not allow multiple events with the same ID

		if ( index >= 0 && index <= Entries.Count )
		{
			Entries.Insert( index, new( id, stageInit, stageShutdown, null ) );
		}

		return this;
	}

	/// <summary>
	/// Adds a startup/shutdown event to the end of the list.
	/// </summary>
	public Orchestrator<T> Add( Enum id, Func<T, bool>? stageInit = null, Action? stageShutdown = null )
		=> Insert( Entries.Count, id, stageInit, stageShutdown );

	/// <summary>
	/// Adds a startup/shutdown event before the enumerated event <paramref name="before"/>.
	/// </summary>
	public Orchestrator<T> Before( Enum before, Enum id, Func<T, bool>? stageInit = null, Action? stageShutdown = null )
		=> Insert( IndexOf( before ), id, stageInit, stageShutdown );

	/// <summary>
	/// Adds a startup/shutdown event after the enumerated event <paramref name="after"/>.
	/// </summary>
	public Orchestrator<T> After( Enum after, Enum id, Func<T, bool>? stageInit = null, Action? stageShutdown = null )
		=> Insert( IndexOf( after ) + 1, id, stageInit, stageShutdown );

	/// <summary>
	/// Inserts an event at the specified index.
	/// </summary>
	public Orchestrator<T> Insert( int index, Enum id, Action other )
	{
		if ( index >= 0 && index <= Entries.Count )
		{
			Entries.Insert( index, new( id, null, null, other ) );
		}

		return this;
	}

	/// <summary>
	/// Adds an event to the end of the list.
	/// </summary>
	public Orchestrator<T> Add( Enum id, Action other )
		=> Insert( Entries.Count, id, other );

	/// <summary>
	/// Adds an event before the enumerated event <paramref name="before"/>.
	/// </summary>
	public Orchestrator<T> Before( Enum before, Enum id, Action other )
		=> Insert( IndexOf( before ), id, other );

	/// <summary>
	/// Adds an event after the enumerated event <paramref name="after"/>.
	/// </summary>
	public Orchestrator<T> After( Enum after, Enum id, Action other )
		=> Insert( IndexOf( after ) + 1, id, other );

	/// <summary>
	/// Inserts a group of events at the specified index.
	/// </summary>
	public Orchestrator<T> Insert( int index, Action<Orchestrator<T>> group )
	{
		if ( index >= 0 && index <= Entries.Count )
		{
			Orchestrator<T> orch = new();
			group( orch );

			Entries.InsertRange( index, orch.Entries );
		}

		return this;
	}

	/// <summary>
	/// Adds a group of events at the end of the list.
	/// </summary>
	public Orchestrator<T> Add( Action<Orchestrator<T>> group )
		=> Insert( Entries.Count, group );

	/// <summary>
	/// Adds a group of events before enumerated event <paramref name="before"/>.
	/// </summary>
	public Orchestrator<T> Before( Enum before, Action<Orchestrator<T>> group )
		=> Insert( IndexOf( before ), group );

	/// <summary>
	/// Adds a group of events before enumerated event <paramref name="after"/>.
	/// </summary>
	public Orchestrator<T> After( Enum after, Action<Orchestrator<T>> group )
		=> Insert( IndexOf( after ) + 1, group );

	/// <summary>
	/// Gets the index of the enumerated event <paramref name="id"/>.
	/// </summary>
	public int IndexOf( Enum id )
		=> Entries.FindIndex( e => e.Id.Equals( id ) );
}
