// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Reflection;
using Elegy.CommandSystem.Helpers;
using Elegy.Common.Utilities;

namespace Elegy.CommandSystem
{
	/// <summary>
	/// Manages instances of <see cref="IConsoleArgumentHelper"/>.
	/// </summary>
	public static class HelperManager
	{
		private static TaggedLogger mLogger = new( "CvarHelper" );

		private static Dictionary<Assembly, List<IConsoleArgumentHelper>> mHelpersByAssembly { get; } = new();

		/// <summary>
		/// See <see cref="IConsoleArgumentHelper"/>.
		/// </summary>
		public static Dictionary<Type, IConsoleArgumentHelper> Helpers { get; } = new();

		/// <summary>
		/// Whether or not there is a helper for a particular <paramref name="type"/>.
		/// </summary>
		public static bool HasHelperForType( Type type ) => Helpers.ContainsKey( type );

		/// <summary>
		/// Registers one <paramref name="helper"/> and optionally ties it to an <paramref name="assembly"/> for bookkeeping.
		/// </summary>
		public static bool RegisterHelper( IConsoleArgumentHelper helper, Assembly? assembly = null )
		{
			var type = helper.Type;

			if ( Helpers.ContainsKey( type ) )
			{
				mLogger.Warning( $"'{type.FullName}' cannot be registered.\nIts type ({type.Name}) is already handled by {Helpers[type].GetType().FullName}." );
				return false;
			}

			Helpers.Add( type, helper );

			if ( assembly is not null )
			{
				if ( !mHelpersByAssembly.ContainsKey( assembly ) )
				{
					mHelpersByAssembly.Add( assembly, new() );
				}

				mHelpersByAssembly[assembly].Add( helper );
			}

			return true;
		}

		/// <summary>
		/// Registers helpers that are provided in an <paramref name="assembly"/>.
		/// </summary>
		public static bool RegisterHelpers( Assembly assembly )
		{
			bool success = true;
			var types = assembly.GetTypes();
			foreach ( var type in types )
			{
				if ( !type.GetInterfaces().Contains( typeof( IConsoleArgumentHelper ) ) )
				{
					continue;
				}

				// It's a little wordy! Oops
				var attribute = type.GetCustomAttribute<ConsoleArgumentHelperAttribute>();
				if ( attribute is null )
				{
					continue;
				}

				var helper = Activator.CreateInstance( type ) as IConsoleArgumentHelper;
				if ( helper is null )
				{
					mLogger.Warning( $"Failed to create instance of '{type.FullName}'" );
					continue;
				}

				if ( !RegisterHelper( helper, assembly ) )
				{
					success = false;
				}
			}

			return success;
		}

		/// <summary>
		/// Unregisters helpers tied to the given <paramref name="assembly"/>.
		/// </summary>
		public static bool UnregisterHelpers( Assembly assembly )
		{
			if ( !mHelpersByAssembly.ContainsKey( assembly ) )
			{
				return false;
			}

			foreach ( var helper in mHelpersByAssembly[assembly] )
			{
				Helpers.Remove( helper.Type );
			}
			mHelpersByAssembly.Remove( assembly );
			return true;
		}

		/// <summary>
		/// Unregisters all helpers.
		/// </summary>
		public static void UnregisterAllHelpers()
		{
			mHelpersByAssembly.Clear();
			Helpers.Clear();
		}
	}
}
