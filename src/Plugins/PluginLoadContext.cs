// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Runtime.Loader;

namespace Elegy
{
	internal class PluginLoadContext : AssemblyLoadContext
	{
		public PluginLoadContext()
			: base( isCollectible: true )
		{

		}

		protected override Assembly? Load( AssemblyName assemblyName )
		{
			Console.Log( "DllLoader", $"'{assemblyName.Name}.dll'", ConsoleMessageType.Verbose );

			// Plugins are likely to be using these primarily
			switch ( assemblyName.Name )
			{
				case "GodotSharp": return typeof( GD ).Assembly;
				case "Elegy.Engine": return typeof( Engine ).Assembly;
				case "Elegy.Common": return typeof( Text.Lexer ).Assembly;
			}

			// Check for any shared dependencies
			//AssemblyLoadContext? currentContext = GetLoadContext( typeof( Engine ).Assembly );
			//if ( currentContext == null )
			//{
			//	return null;
			//}

			//foreach ( var assembly in currentContext.Assemblies )
			//{
			//	if ( assembly.IsDynamic )
			//	{
			//		continue;
			//	}
			//
			//	if ( assembly.GetName().Name == assemblyName.Name )
			//	{
			//		return assembly;
			//	}
			//}

			// TODO: check for assemblies in plugin system

			// Fallback to the host context
			return null;
		}
	}
}
