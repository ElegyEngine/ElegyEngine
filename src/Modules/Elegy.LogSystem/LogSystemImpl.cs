using Elegy.Common.Interfaces.Services;
using L = Elegy.LogSystem.API.Log;

namespace Elegy.LogSystem;

internal class LogSystemImpl : ILogSystem
{
	public bool IsDeveloper
	{
		get => L.Developer;
		set => L.Developer = value;
	}

	public bool IsVerbose
	{
		get => L.Verbose;
		set => L.Verbose = value;
	}

	public void Log( string message )
		=> L.Submit( message );

	public void Warning( string message )
		=> L.Warning( message );

	public void Error( string message )
		=> L.Error( message );

	public void Fatal( string message )
		=> L.Fatal( message );

	public void Developer( string message )
		=> L.Submit( message, ConsoleMessageType.Developer );

	public void Verbose( string message )
		=> L.Submit( message, ConsoleMessageType.Verbose );

	public void Success( string message )
		=> L.Success( message );

	public void Log( string tag, string message )
		=> L.Submit( tag, message );

	public void Warning( string tag, string message )
		=> L.Warning( tag, message );

	public void Error( string tag, string message )
		=> L.Error( tag, message );

	public void Fatal( string tag, string message )
		=> L.Fatal( tag, message );

	public void Developer( string tag, string message )
		=> L.Submit( tag, message, ConsoleMessageType.Developer );

	public void Verbose( string tag, string message )
		=> L.Submit( tag, message, ConsoleMessageType.Verbose );

	public void Success( string tag, string message )
		=> L.Success( tag, message );
}
