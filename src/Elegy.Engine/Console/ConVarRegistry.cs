
namespace Elegy
{
	/// <summary>
	/// Provides utilities for static ConVars and ConCommands.
	/// </summary>
	internal class ConVarRegistry
	{
		public List<ConCommand> Commands { get; } = new();

		public ConVarRegistry( Assembly assembly )
		{
			var types = assembly.GetTypes();
			for ( int i = 0; i < types.Length; i++ )
			{
				var type = types[i];
				var staticMembers = type.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Static );
				foreach ( var staticMember in staticMembers )
				{
					if ( staticMember.GetType() == typeof( ConCommand ) )
					{
						ConCommand? cmd = staticMember.GetValue( null ) as ConCommand;
						Commands.Add( cmd );
					}
				}
			}
		}

		public void RegisterAll()
		{
			Commands.ForEach( cmd => Console.RegisterCommand( cmd ) );
		}

		public void UnregisterAll()
		{
			Commands.ForEach( cmd => Console.UnregisterCommand( cmd ) );
		}
	}
}
