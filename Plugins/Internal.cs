
namespace Elegy
{
	public static partial class Plugins
	{
		internal static void SetPluginSystem( PluginSystem pluginSystem )
		{
			mPluginSystem = pluginSystem;
		}

		private static PluginSystem mPluginSystem;
	}
}
