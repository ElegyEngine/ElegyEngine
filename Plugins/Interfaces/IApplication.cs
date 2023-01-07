
namespace Elegy
{
	public interface IApplication : IPlugin
	{
		// Start up game/app systems after all plugins have loaded
		bool Start();
		// Execute a single game frame
		bool RunFrame( float delta );
	}
}
