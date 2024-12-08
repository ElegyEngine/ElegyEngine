// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Game.Presentation
{
	public class GamePresentation
	{
		public Renderer Renderer { get; set; } = new();

		public bool Init()
		{
			if ( !Renderer.Init() )
			{
				return false;
			}

			return true;
		}

		public void Shutdown()
		{
			Renderer.Shutdown();
		}

		public void Update( float delta )
		{
		}
	}
}
