// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.LogSystem;
using Elegy.PlatformSystem.API;
using Elegy.RenderBackend;
using Elegy.RenderBackend.Assets;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Veldrid;

namespace Game.Presentation
{
	public partial class Renderer
	{
		private CommandList mRenderCommands;
		private static TaggedLogger mLogger = new( "WorldRender" );

		public SurfaceCache OpaqueCache { get; } = new();
		public SurfaceCache TransparentCache { get; } = new();
		
		public Action<View> OnSubmitSurfaces { get; set; } = _ => { };
		public Action<View, CommandList> OnFrameEnd { get; set; } = ( _, _ ) => { };

		public void RenderView( View view )
		{
			// Gather all render surfaces
			// It is possible to cache these for static props per map area, and
			// regenerate dynamic ones instead this way. Also, occlusion culling
			ClearRenderSurfaces();
			OnSubmitSurfaces( view );

			// Currently we have no lights, so oops
			ReadOnlySpan<Light> lights = [];

			// Update all buffers at the start of the frame pretty please
			var updateBuffersTask = Task.Run( Render.UpdateBuffers );

			// Begin rendering
			mRenderCommands.Begin();
			Render.SetRenderView( mRenderCommands, view );

			// Opaque pass
			foreach ( var item in OpaqueCache.Cache.Span )
			{
				Render.RenderStyle.RenderSurfaces( mRenderCommands, view, item.Surfaces.Span, item.Material, lights );
			}

			// Transparent pass
			foreach ( var item in TransparentCache.Cache.Span )
			{
				Render.RenderStyle.RenderSurfaces( mRenderCommands, view, item.Surfaces.Span, item.Material, lights );
			}

			// Draw UI etc.
			OnFrameEnd( view, mRenderCommands );
			mRenderCommands.End();

			// Wait for the command buffers to be updated now, before we move on to actually drawing
			updateBuffersTask.Wait();

			// Finally, draw the scene
			Render.Device.SubmitCommands( mRenderCommands );
		}

		/// <summary>
		/// The render loop.
		/// </summary>
		public void RenderFrame()
		{
			var window = Platform.GetCurrentWindow();
			if ( window is null )
			{
				return;
			}

			View? currentView = Render.GetView( window );
			if ( currentView is null )
			{
				return;
			}

			if ( Render.RenderStyle is null )
			{
				return;
			}

			RenderView( currentView );
		}

		public void QueueMeshEntity( MeshEntity meshEntity )
		{
			var dataBlock = meshEntity.System.Chunks[meshEntity.ChunkIndex].Blocks[meshEntity.ElementIndex];

			var meshes = dataBlock.Mesh.Submeshes;
			for ( int meshIndex = 0; meshIndex < meshes.Count; meshIndex++ )
			{
				QueueRenderSurface(
					meshes[meshIndex],
					dataBlock.EntitySet,
					dataBlock.InstanceParameterPool[meshIndex],
					dataBlock.Mesh.Materials[meshIndex] );
			}
		}

		public void QueueRenderSurface( ArrayMesh mesh, ResourceSet set, MaterialParameterPool pool, RenderMaterial material )
		{
			var materialMap = material.Template.Data.PipelineInfo.BlendMode switch
			{
				Blending.Opaque => OpaqueCache,
				_               => TransparentCache
			};

			materialMap.GetOrAdd( material ).Add( new()
			{
				Mesh = mesh,
				ParameterPool = pool,
				PerEntitySet = set
			} );
		}

		private void ClearRenderSurfaces()
		{
			OpaqueCache.Clear();
			TransparentCache.Clear();
		}
	}
}
