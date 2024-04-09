// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.PluginSystem.API;

using Silk.NET.Windowing;

namespace Elegy.PlatformSystem.API
{
	/// <summary>
	/// Platforming system. Handles windowing and provides access to input.
	/// </summary>
	public static partial class Platform
	{
		public static bool Init( in LaunchConfig config )
		{
			mWindowPlatform = null;

			Plugins.RegisterDependency( "Elegy.PluginSystem", typeof( Platform ).Assembly );

			return true;
		}

		public static void Set( IWindowPlatform? windowPlatform )
		{
			mWindowPlatform = windowPlatform;
		}

		public static void Shutdown()
		{

		}
	}
}
