// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Text;

namespace Elegy.Common.Tests.Text;

public static class LexerTests
{
	[Fact]
	public static void LexerBasicTest()
	{
		Lexer lex = new( string.Empty );
		KeyvalueParsingTest( lex );
	}

	[Fact]
	public static void Lexer2BasicTest()
	{
		Lexer2 lex = new( string.Empty );
		KeyvalueParsingTest( lex );
	}
	
	internal static void KeyvalueParsingTest( ILexer lex )
	{
		const string inputString =
			"""
			object {
				key1 value1
				key2 value2
				key3 value3
				key4 {
					hello 123
				}
				
				key5 12.5
			} // this is a comment
			""";

		lex.Load( inputString );

		Assert.True( lex.Next() is "object" );
		Assert.True( lex.Next() is "{" );
		Assert.True( lex.Next() is "key1" );
		Assert.True( lex.Next() is "value1" );
		Assert.True( lex.Next() is "key2" );
		Assert.True( lex.Next() is "value2" );
		Assert.True( lex.Next() is "key3" );
		Assert.True( lex.Next() is "value3" );
		Assert.True( lex.Next() is "key4" );
		Assert.True( lex.Next() is "{" );
		Assert.True( lex.Next() is "hello" );
		Assert.True( lex.Next() is "123" );
		Assert.True( lex.Next() is "}" );
		Assert.True( lex.Next() is "key5" );
		Assert.True( lex.Next() is "12.5" );
		Assert.True( lex.Next() is "}" );
		Assert.True( lex.Next() is "" );
		Assert.True( lex.IsEnd() );
	}
}
