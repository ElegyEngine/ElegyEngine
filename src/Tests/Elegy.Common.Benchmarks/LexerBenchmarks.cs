
using System.Text;
using BenchmarkDotNet.Attributes;
using Elegy.Common.Text;

namespace Elegy.Common.Benchmarks;

[MemoryDiagnoser]
public class LexerBenchmarks
{
	private string mTextInputSmall = string.Empty;
	private string mTextInputLarge = string.Empty;
	
	[GlobalSetup]
	public void Prepare()
	{
		StringBuilder sb = new();

		// Generate a basic JSON-like keyvalue text with some 2000 keyvalues
		for ( int i = 0; i < 2000; i++ )
		{
			int randomKey = Random.Shared.Next();
			int randomValue = Random.Shared.Next();

			sb.AppendLine( $"{{ key{randomKey} value{randomValue} }}" );
		}
		
		mTextInputSmall = sb.ToString();

		sb.Clear();
		for ( int i = 0; i < 40000; i++ )
		{
			int randomKey = Random.Shared.Next();
			int randomValue = Random.Shared.Next();

			sb.AppendLine( $"{{ key{randomKey} value{randomValue} someRandomTextHere {randomValue * randomKey} }}" );
		}
		
		mTextInputLarge = sb.ToString();
	}

	[Benchmark]
	public int LexerString_Small()
	{
		Lexer lex = new( mTextInputSmall );
		while ( !lex.IsEnd() )
		{
			lex.Next();
		}

		return lex.GetLineNumber();
	}
	
	[Benchmark]
	public int LexerSpan_Small()
	{
		Lexer2 lex = new( mTextInputSmall );
		while ( !lex.IsEnd() )
		{
			lex.Next();
		}

		return lex.GetLineNumber();
	}
	
	[Benchmark]
	public int LexerString_Large()
	{
		Lexer lex = new( mTextInputLarge );
		while ( !lex.IsEnd() )
		{
			lex.Next();
		}

		return lex.GetLineNumber();
	}
	
	[Benchmark]
	public int LexerSpan_Large()
	{
		Lexer2 lex = new( mTextInputLarge );
		while ( !lex.IsEnd() )
		{
			lex.Next();
		}

		return lex.GetLineNumber();
	}
}
