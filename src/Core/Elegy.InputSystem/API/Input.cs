// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.ConsoleSystem;
using Elegy.PluginSystem.API;

namespace Elegy.InputSystem.API
{
	/// <summary>
	/// Simple access to input functions. Key presses, mouse coordinates etc.
	/// </summary>
	public static partial class Input
	{
		private static TaggedLogger mLogger = new( "Input" );

		public static bool Init( in LaunchConfig config )
		{
			mLogger.Log( "Init" );

			Plugins.RegisterDependency( "Elegy.InputSystem", typeof( Input ).Assembly );

			return true;
		}

		public static void PostInit()
		{

		}

		public static void Shutdown()
		{
			mLogger.Log( "Shutdown" );
		}
	}
}
