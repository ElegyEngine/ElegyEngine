// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using System.Numerics;

namespace Elegy.RenderWorld
{
	public partial class RenderWorld
	{
		public List<MeshEntity> MeshEntities { get; } = new( 4096 );

		public int CreateEntity( bool animated, Mesh mesh, Vector3 position, Vector3 angles )
		{
			var renderEntity = Render.CreateEntity( animated );
			renderEntity.Mesh = mesh;

			MeshEntities.Add( renderEntity );
			return MeshEntities.Count - 1;
		}
	}
}
