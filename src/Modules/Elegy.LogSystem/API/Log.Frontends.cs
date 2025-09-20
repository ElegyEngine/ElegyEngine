// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.LogSystem.Frontends;

namespace Elegy.LogSystem.API
{
	public static partial class Log
	{
		/// <summary>
		/// Registers a <seealso cref="IConsoleFrontend"/>.
		/// </summary>
		public static bool AddFrontend( IConsoleFrontend frontend )
		{
			if ( mFrontends.Contains( frontend ) )
			{
				mLogger.Verbose( $"Frontend '{frontend.Name}' already added" );
				return true;
			}

			// Since console frontends are plugins, they can be often times initialised by the plugin system
			// Sometimes, however, somebody may simply call Log.AddFrontend( new MyFrontend() );, in
			// which case we initialise it here.
			if ( !frontend.Initialised )
			{
				frontend.Init();
			}

			if ( frontend.Error == string.Empty )
			{
				mFrontends.Add( frontend );
				mLogger.Developer( $"Added frontend '{frontend.Name}'" );
				return true;
			}

			mLogger.Warning( $"'{frontend.Name}' failed to initialise with message: '{frontend.Error}'" );
			return false;
		}

		/// <summary>
		/// Unregisters a <seealso cref="IConsoleFrontend"/>.
		/// </summary>
		public static bool RemoveFrontend( IConsoleFrontend frontend )
		{
			if ( !mFrontends.Exists( internalFrontend => internalFrontend == frontend ) )
			{
				mLogger.Warning( $"Frontend '{frontend.Name}' is already removed" );
				return false;
			}

			if ( frontend.Initialised )
			{
				frontend.Shutdown();
			}

			mFrontends.Remove( frontend );
			mLogger.Log( $"Removed frontend '{frontend.Name}'" );
			return true;
		}
	}
}
