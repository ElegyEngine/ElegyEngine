// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.ConsoleSystem.Commands;
using Elegy.ConsoleSystem.Frontends;
using System.Diagnostics;

namespace Elegy.ConsoleSystem.API
{
	public static partial class Console
	{
		public static bool Init( in LaunchConfig launchConfig )
		{
			mTimer.Restart();

			InitialiseArguments( launchConfig.Args );

			// Log as early as possible
			AddFrontend( new SystemConsoleFrontend() );
			if ( launchConfig.ConsoleFrontends is not null )
			{
				foreach ( var frontend in launchConfig.ConsoleFrontends )
				{
					Debug.Assert( frontend is IConsoleFrontend );
					AddFrontend( frontend as IConsoleFrontend );
				}
			}

			mLogger.Log( "Init" );

			Verbose = mArguments.GetBool( "-verbose" );
			Developer = Verbose || mArguments.GetBool( "-developer" );

			HelperManager.RegisterHelpers( Assembly.GetExecutingAssembly() );

			return true;
		}

		public static void InitAssemblyConvars( Assembly assembly )
		{
			HelperManager.RegisterHelpers( assembly );

			mEngineConvarRegistry = new( assembly );
			mEngineConvarRegistry.RegisterAll();
		}

		public static void Update( float delta )
		{
			for ( int i = 0; i < mFrontends.Count; i++ )
			{
				mFrontends[i].OnUpdate( delta );
			}
		}

		public static void Shutdown()
		{
			mLogger.Log( "Shutdown" );

			foreach ( var frontend in mFrontends )
			{
				if ( frontend.Initialised )
				{
					frontend.Shutdown();
				}
			}

			mFrontends.Clear();
			mArguments.Clear();

			mEngineConvarRegistry?.UnregisterAll();
			mEngineConvarRegistry = null;
		}
	}
}
