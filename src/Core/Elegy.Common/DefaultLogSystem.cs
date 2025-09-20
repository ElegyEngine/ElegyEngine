// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Interfaces.Services;

namespace Elegy.Common;

public class DefaultLogSystem : ILogSystem
{
	public bool IsDeveloper { get; set; }
	public bool IsVerbose { get; set; }

	public void Log( string message )
	{
		Console.WriteLine( message );
	}

	public void Log( string tag, string message )
		=> Log( $"[{tag}] {message}" );

	public void Warning( string message )
	{
		PrintColour( message, ConsoleColor.Yellow );
	}

	public void Warning( string tag, string message )
		=> Warning( $"[{tag}] {message}" );

	public void Error( string message )
	{
		PrintColour( message, ConsoleColor.Red );
	}

	public void Error( string tag, string message )
		=> Error( $"[{tag}] {message}" );

	public void Fatal( string message )
	{
		PrintColour( $"FATAL: {message}", ConsoleColor.White, ConsoleColor.Red );
	}

	public void Fatal( string tag, string message )
		=> Fatal( $"[{tag}] {message}" );

	public void Developer( string message )
	{
		if ( IsDeveloper )
		{
			PrintColour( message, ConsoleColor.DarkCyan );
		}
	}

	public void Developer( string tag, string message )
		=> Developer( $"[{tag}] {message}" );

	public void Verbose( string message )
	{
		if ( IsVerbose )
		{
			PrintColour( $"TRACE: {message}", ConsoleColor.Gray );
		}
	}

	public void Verbose( string tag, string message )
		=> Verbose( $"[{tag}] {message}" );

	public void Success( string message )
	{
		PrintColour( message, ConsoleColor.White, ConsoleColor.DarkGreen );
	}

	public void Success( string tag, string message )
		=> Success( $"[{tag}] {message}" );

	private void PrintColour( string message, ConsoleColor colour, ConsoleColor background = ConsoleColor.Black )
	{
		Console.ForegroundColor = colour;
		Console.BackgroundColor = background;
		Console.WriteLine(message);
		Console.ResetColor();
	}
}
