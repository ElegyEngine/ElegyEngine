// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.AssetSystem.API;
using Elegy.Common.Assets;
using Elegy.Common.Maths;
using Elegy.ECS;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Game.Presentation;
using Game.Server;

namespace Game.Shared.Components
{
	[Component]
	[Requires<Transform>]
	public partial struct StaticModel
	{
		public MeshEntity? MeshEntity { get; set; } // not null on clientside
		[Property] public ModelProperty Model { get; set; } // smart keyvalue that loads models

		[Event]
		public void ClientSpawn( Entity.ClientSpawnEvent data )
		{
			if ( Model.Data is null )
			{
				return;
			}

			ref var transform = ref data.Self.Ref<Transform>();
			MeshEntity = CreateMeshEntity( false, Model.Data, transform.Position, Vector3.Zero );
		}

		[Event]
		public void OnRender( Renderer.RenderEvent data )
		{
			data.RenderContext.QueueMeshEntity( MeshEntity );
		}

		public void SetModel( string name )
			=> Model.SetModel( name );

		public static MeshEntity CreateMeshEntity( bool animated, Model modelData, Vector3 position, Vector3 angles )
		{
			// TODO: Reuse meshes
			var renderEntity = Render.CreateEntity( animated );
			renderEntity.Mesh = Render.CreateMesh( modelData );

			renderEntity.Transform = Coords.CreateWorldMatrixDegrees( position, angles );

			return renderEntity;
		}
	}
}
