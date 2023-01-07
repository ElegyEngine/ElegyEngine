
namespace Elegy
{
	public static partial class Console
	{
		public static void Log( string message = "" ) => mConsole.Log( $"{message}\n" );

		public static void LogInline( string message = "" ) => mConsole.Log( message );

		public static void Warning( string message ) => mConsole.Warning( message );

		public static void Error( string message ) => mConsole.Error( message );

		public static void Fatal( string message ) => mConsole.Error( message, true );

		public static void LogArray<T>( params T[] values )
		{
			for ( int i = 0; i < values.Length; i++ )
			{
				if ( i != 0 )
				{
					LogInline( ", " );
				}

				LogInline( $"{values[i]}" );
			}

			Log();
		}
	}
}
