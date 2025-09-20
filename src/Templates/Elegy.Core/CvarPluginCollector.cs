using System.Reflection;
using Elegy.CommandSystem;
using Elegy.Common.Interfaces;
using Elegy.Common.Utilities;
using Elegy.LogSystem;

namespace Elegy.Core;

/// <summary>
/// Handles the registration of convars and commands from plugins as they come and go.
/// </summary>
public class CvarPluginCollector : IPluginCollector
{
	private TaggedLogger mLogger = new( "Comms" );

	public Dictionary<IPlugin, ConVarRegistry> Registries { get; } = new();

	public void OnAssemblyLoaded( Assembly assembly )
	{
		if ( !HelperManager.RegisterHelpers( assembly ) )
		{
			mLogger.Warning( $"'{assembly.GetName().Name}.dll' has one or more console arg. helpers that failed to load, some console commands may not work!" );
		}
	}

	public void OnPluginLoaded( IPlugin plugin )
	{
		ConVarRegistry registry = new( plugin.GetType().Assembly, plugin );
		registry.RegisterAll();

		Registries.Add( plugin, registry );
	}

	public void OnPluginUnloaded( IPlugin plugin )
	{
		Registries[plugin].UnregisterAll();
		Registries.Remove( plugin );

		HelperManager.UnregisterHelpers( plugin.GetType().Assembly );
	}
}
