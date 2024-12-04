
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Elegy.Common.Benchmarks;

public static class Program
{
	public static void Main( string[] args )
	{
		var config = DefaultConfig.Instance
			.WithOptions( ConfigOptions.DisableOptimizationsValidator );
		
		BenchmarkRunner.Run<LexerBenchmarks>( config );
	}
}
