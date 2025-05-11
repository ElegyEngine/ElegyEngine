// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Elegy.Common.Maths;
using Elegy.ConsoleSystem;
using Elegy.ECS;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Game.Presentation;

namespace Game.Shared.Components
{
	[Component]
	[Requires<Transform>]
	public partial struct StaticModel
	{
		private static TaggedLogger mLogger = new( "StaticModel" );
		private MeshEntity mMeshEntity;

		// Not null on clientside
		public MeshEntity MeshEntity => mMeshEntity;

		// Smart keyvalue that loads models
		[Property] public ModelProperty Model { get; set; }

		[Event]
		public void ClientSpawn( Entity.ClientSpawnEvent data )
		{
			if ( Model.Data is null )
			{
				return;
			}

			ref var transform = ref data.Self.Ref<Transform>();
			mMeshEntity = CreateMeshEntity( false, Model.Data, transform.Position, transform.Orientation );
		}

		[GroupEvent]
		public static void ClientUpdate( Entity.ClientUpdateEvent data, ref StaticModel model, ref Transform transform )
		{
			if ( !transform.TransformDirty )
			{
				//return;
			}

			model.mMeshEntity.Transform = Coords.CreateWorldMatrixQuaternion( transform.Position, transform.Orientation );
		}

		[GroupEvent]
		public static void OnDebugDraw( Entity.DebugDrawEvent data, ref StaticModel model, ref Transform transform )
		{
			Vector3 start = transform.Position;
			Vector3 up = start + Coords.Up * 0.33f;
			Vector3 down = start + Coords.Down * 0.33f;
			Vector3 forward = start + Coords.Forward * 0.33f;
			Vector3 back = start + Coords.Back * 0.33f;
			Vector3 left = start + Coords.Left * 0.33f;
			Vector3 right = start + Coords.Right * 0.33f;
			Vector4 colour = new( 0.33f, 0.7f, 0.33f, 1.0f );
			
			Render.DebugLine( up, down, colour );
			Render.DebugLine( forward, back, colour );
			Render.DebugLine( left, right, colour );
		}
		
		[GroupEvent]
		public static void OnRender( Renderer.RenderEvent data, ref StaticModel model )
		{
			data.RenderContext.QueueMeshEntity( model.mMeshEntity );
		}

		public void SetModel( string name )
			=> Model.SetModel( name );

		public static MeshEntity CreateMeshEntity( bool animated, Model modelData, Vector3 position, Quaternion angles )
		{
			// TODO: Reuse meshes
			var renderEntity = Render.CreateEntity( animated );
			renderEntity.Mesh = Render.CreateMesh( modelData );

			renderEntity.Transform = Coords.CreateWorldMatrixQuaternion( position, angles );

			return renderEntity;
		}
	}
}
