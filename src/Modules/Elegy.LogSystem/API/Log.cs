// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using System.Diagnostics;
using Elegy.Common.Interfaces;
using Elegy.Common.Interfaces.Services;
using Elegy.Common.Utilities;
using Elegy.LogSystem.Frontends;

namespace Elegy.LogSystem.API
{
	public static partial class Log
	{
		/// <summary>
		/// Initialises the logging subsystem.
		/// </summary>
		public static bool Init( IPlugin[]? consoleFrontends )
		{
			mTimer.Restart();
			ElegyInterfaceLocator.Set<ILogSystem>( new LogSystemImpl() );

			// Submit as early as possible
			// TODO: Move frontend capturing out of here
			AddFrontend( new SystemConsoleFrontend() );
			if ( consoleFrontends is not null )
			{
				foreach ( var frontend in consoleFrontends )
				{
					Debug.Assert( frontend is IConsoleFrontend );
					AddFrontend( (IConsoleFrontend)frontend );
				}
			}

			mLogger.Log( "Init" );
			return true;
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
		}
	}
}
