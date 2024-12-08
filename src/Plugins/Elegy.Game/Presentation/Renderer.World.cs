// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Collections.Pooled;
using Elegy.ECS;
using Elegy.PlatformSystem.API;
using Elegy.RenderBackend;
using Elegy.RenderBackend.Assets;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
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
			EntityWorld.ForEachEntity( entity =>
			{
				entity.Dispatch( new RenderEvent( this ) );
			} );
		}
	}
}
