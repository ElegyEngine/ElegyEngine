// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.Common.Interfaces.Services;

/// <summary>
/// Elegy.LogSystem module decoupler. A lot of engine modules
/// depend on the logging system, however, they don't exactly
/// have to with dependency injection.
///
/// When used in isolation, the modules will simply log using
/// System.Console. When used with the engine, it'll use
/// Elegy.LogSystem. :)
/// </summary>
public interface ILogSystem
{
	bool IsDeveloper { get; set; }
	bool IsVerbose { get; set; }

	void Log( string message );
	void Warning( string message );
	void Error( string message );
	void Fatal( string message );
	void Developer( string message );
	void Verbose( string message );
	void Success( string message );

	void Log( string tag, string message );
	void Warning( string tag, string message );
	void Error( string tag, string message );
	void Fatal( string tag, string message );
	void Developer( string tag, string message );
	void Verbose( string tag, string message );
	void Success( string tag, string message );
}
