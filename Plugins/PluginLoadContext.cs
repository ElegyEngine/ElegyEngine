
using System.Runtime.Loader;

namespace Elegy
{
	internal class PluginLoadContext : AssemblyLoadContext
	{
		protected override Assembly? Load( AssemblyName assemblyName )
		{
			// Plugins are likely to be using these primarily
			switch ( assemblyName.Name )
			{
				case "GodotSharp": return typeof( GD ).Assembly;
				case "Elegy.Engine": return typeof( Engine ).Assembly;
			}

			// Check for any shared dependencies
			AssemblyLoadContext? currentContext = GetLoadContext( typeof( Engine ).Assembly );
			if ( currentContext == null )
			{
				return null;
			}

			foreach ( var assembly in currentContext.Assemblies )
			{
				if ( assembly.IsDynamic )
				{
					continue;
				}

				AssemblyName otherAssemblyName = new( assembly.FullName );
				if ( assemblyName.Name == otherAssemblyName.Name )
				{
					return assembly;
				}	
			}

			// Fallback to the host context
			return null;
		}
	}
}
