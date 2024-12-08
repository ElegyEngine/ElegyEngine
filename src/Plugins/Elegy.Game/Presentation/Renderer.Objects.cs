// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;

namespace Game.Presentation
{
	public partial class Renderer
	{
		public static MeshEntity CreateEntity( bool animated, Mesh mesh, Vector3 position, Vector3 angles )
		{
			var renderEntity = Render.CreateEntity( animated );
			renderEntity.Mesh = mesh;

			renderEntity.Transform = Coords.CreateWorldMatrixDegrees( position, angles );

			return renderEntity;
		}
	}
}
