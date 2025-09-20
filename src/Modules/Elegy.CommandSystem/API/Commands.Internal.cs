// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces.Services;
using Elegy.Common.Utilities;

namespace Elegy.CommandSystem.API;

public static partial class Commands
{
	private static TaggedLogger mLogger = new( "Commands" );
	internal static ILogSystem Log => ElegyInterfaceLocator.GetLogSystem();

	private static ConVarRegistry? mConvars;
	private static Dictionary<string, ConsoleCommand> mCommands = new();
	private static Dictionary<string, string> mArguments = new();

	private static void InitialiseArguments( string[] args )
	{
		if ( args.Length == 0 )
		{
			mLogger.Verbose( "Launch arguments: empty" );
			return;
		}

		var isKey = ( string text ) => text.StartsWith( "-" ) || text.StartsWith( "+" );

		mLogger.Verbose( "Launch arguments:" );

		for ( int i = 0; i < args.Length; i++ )
		{
			if ( isKey( args[i] ) )
			{
				string value = "1";

				if ( i < args.Length - 1 && !isKey( args[i + 1] ) )
				{
					value = args[i + 1];
				}

				mArguments[args[i]] = value;
				mLogger.Verbose( $"   * '{args[i]}' = '{value}'" );
			}
		}
	}
}
