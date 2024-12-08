// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.API;

namespace Game.Presentation
{
	public partial class Renderer
	{
		public bool Init()
		{
			mRenderCommands = Render.Factory.CreateCommandList();
			Render.OnRender += RenderFrame;
			OnSubmitSurfaces += SubmitSurfacesWorld;
			
			return true;
		}

		public void Shutdown()
		{
			Render.OnRender -= RenderFrame;
		}
	}
}
