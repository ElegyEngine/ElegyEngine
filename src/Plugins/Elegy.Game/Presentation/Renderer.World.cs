// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;
using Elegy.RenderSystem.Objects;
using Game.Shared;

namespace Game.Presentation
{
	public partial class Renderer
	{
		[EventModel]
		public record struct RenderEvent( Renderer RenderContext );

		public void SubmitSurfacesWorld( View view )
		{
			// Right now it's just a test: we draw anything and everything
			EntityWorld.Dispatch( new RenderEvent( this ) );
		}
	}
}
