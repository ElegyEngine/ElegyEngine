// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy;

public partial class Engine
{
	[ConsoleCommand( "test" )]
	public static bool Command_Test( int a, int b = 20 )
	{
		Console.Log( $"You've successfully called 'test' with {a} and {b}!" );
		return true;
	}

	//[ConsoleCommand( "test_badparams" )]
	public static bool Command_BadParameters( byte a, short b, long c, Half d, DateTime e )
	{
		return true;
	}

	//[ConsoleCommand( "test_badreturn" )]
	public static int Command_BadReturnType( int a )
	{
		return 0;
	}

	[ConsoleCommand( "test_args" )]
	public static void Command_OnlyArgs( string[] args )
	{

	}

	[ConsoleCommand( "test_noparams" )]
	public static void Command_NoParameters()
	{

	}

	[ConsoleCommand( "test_nonstatic" )]
	public void Command_NonStatic()
	{

	}
}
