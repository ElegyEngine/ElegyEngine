// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AppTemplate;
using Silk.NET.Windowing;

namespace Elegy.Launcher2
{
	internal static class Program
	{
		static void Main( string[] args )
		{
			Console.Title = "Elegy.Launcher2";
			Window.PrioritizeSdl();

			Application.Start(
				config:
					new()
					{
						Args = args,
						EngineConfigName = null,
						Engine = new(), // use default configuration
						WithMainWindow = true
					},

				windowPlatform:
					Window.GetWindowPlatform( viewOnly: false )
					?? throw new Exception( "SDL2 not found" )
			);
		}
	}
}