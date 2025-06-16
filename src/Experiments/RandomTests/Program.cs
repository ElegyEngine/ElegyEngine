using System.Collections.Concurrent;
using System.Diagnostics;


for ( int i = 0; i < 10; i++ )
{
	Test.Run();
	Profiler.Reset();
}

Test.Run();
Test.PrintProfilingData();

public static class Profiler
{
	private static readonly Stopwatch mStopwatch = Stopwatch.StartNew();

	public static bool Enabled { get; set; } = true;

	public static ProfilerNode Root { get; } = new()
	{
		Name = "Root",
		TimeStart = 0.0,
		TimeEnd = 0.0,
	};

	public static ConcurrentDictionary<int, ProfilerNode> Current { get; } = new();

	public static ProfileScopeWrapper Scope( string name )
		=> new( name );

	public static void Marker( string name )
	{
		if ( !Enabled )
		{
			return;
		}

		Push( name );

		int threadId = Thread.CurrentThread.ManagedThreadId;
		ProfilerNode current = Current.GetValueOrDefault( threadId, Root );

		Pop();

		current.TimeEnd = current.TimeStart;
	}

	public static void Scope( string name, Action action )
	{
		if ( !Enabled )
		{
			action();
			return;
		}

		Push( name );
		action();
		Pop();
	}

	public static void Push( string regionName )
	{
		if ( !Enabled )
		{
			return;
		}

		int threadId = Thread.CurrentThread.ManagedThreadId;
		ProfilerNode current = Current.GetValueOrDefault( threadId, Root );
		ProfilerNode child = new()
		{
			Name = regionName,
			ThreadName = Thread.CurrentThread.Name ?? $"Thread #{threadId}",
			Parent = current
		};

		current.AddChild( child );
		Current[threadId] = child;

		child.TimeStart = CurrentTime;
	}

	public static void Pop()
	{
		if ( !Enabled )
		{
			return;
		}

		// Sample it here to avoid overhead
		double endTime = CurrentTime;

		int threadId = Thread.CurrentThread.ManagedThreadId;
		ProfilerNode current = Current.GetValueOrDefault( threadId, Root );

		current.TimeEnd = endTime;
		Current[threadId] = current.Parent ?? Root;
	}

	public static void Reset() => Root.Clear();

	public static double CurrentTime => mStopwatch.Elapsed.TotalMicroseconds / (1000.0 * 1000.0);
}

public class ProfilerNode
{
	private readonly object mLock = new(); // TODO: use System.Threading.Lock from .NET 9
	public List<ProfilerNode> Children { get; } = new();
	public ProfilerNode? Parent { get; set; }

	public void AddChild( ProfilerNode child )
	{
		lock ( mLock )
		{
			Children.Add( child );
		}
	}

	public void Clear()
	{
		foreach ( var child in Children )
		{
			child.Clear();
		}

		Children.Clear();
	}

	public double TimeStart { get; set; }
	public double TimeEnd { get; set; }

	public string Name { get; set; } = string.Empty;
	public string ThreadName { get; set; } = string.Empty;

	public bool IsMarker => TimeStart == TimeEnd;
}

public struct ProfileScopeWrapper : IDisposable
{
	public ProfileScopeWrapper( string name )
	{
		Profiler.Push( name );
	}

	public void Dispose()
	{
		Profiler.Pop();
	}
}

public static class Test
{
	public static void Run()
	{
		Console.WriteLine( "Running Test" );

		Profiler.Marker( "Main Marker" );
		using ( Profiler.Scope( "Task 1" ) )
		{
			Console.WriteLine( "Performing Task 1..." );

			Task.Run( static () =>
				Profiler.Scope( "Async Task", static () =>
					Console.WriteLine( "Performing Async Task..." ) ) );

			using ( Profiler.Scope( "Task 2" ) )
			{
				Console.WriteLine( "Performing Task 2..." );
			}
		}

		Profiler.Marker( "Other Marker" );
		Profiler.Scope( "Other Task", static () => Console.WriteLine( "Performing e3" ) );

		Console.WriteLine( "Exiting Test..." );

		Debug.Assert( Profiler.Root == Profiler.Current[Thread.CurrentThread.ManagedThreadId] );
	}

	public static void PrintProfilingData()
	{
		void PrintNode( ProfilerNode node, int level )
		{
			double elapsed = node.TimeEnd - node.TimeStart;

			for ( int i = 0; i < level; i++ )
			{
				Console.Write( "  " );
			}

			if ( node.IsMarker )
			{
				Console.WriteLine( $" ! {node.Name} [{node.ThreadName}]" );
			}
			else
			{
				Console.WriteLine( $" * {node.Name} ({elapsed * 1000.0 * 1000.0:F2} us) [{node.ThreadName}]" );
			}

			foreach ( var child in node.Children )
			{
				PrintNode( child, level + 1 );
			}
		}

		PrintNode( Profiler.Root, 0 );
	}
}
