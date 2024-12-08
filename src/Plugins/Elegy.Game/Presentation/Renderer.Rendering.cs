// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Collections.Pooled;
using Elegy.PlatformSystem.API;
using Elegy.RenderBackend;
using Elegy.RenderBackend.Assets;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Veldrid;

namespace Game.Presentation
{
	public partial class Renderer
	{
		private CommandList mRenderCommands;

		public PooledDictionary<RenderMaterial, PooledList<RenderSurface>> mOpaqueMaterialMap = new( capacity: 512 );
		public PooledDictionary<RenderMaterial, PooledList<RenderSurface>> mTransparentMaterialMap = new( capacity: 512 );

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
			Render.UpdateBuffers();

			// Begin rendering
			mRenderCommands.Begin();
			Render.SetRenderView( mRenderCommands, view );

			// Opaque pass
			foreach ( var item in mOpaqueMaterialMap )
			{
				Render.RenderStyle.RenderSurfaces( mRenderCommands, view, item.Value.Span, item.Key, lights );
			}

			// Transparent pass
			foreach ( var item in mTransparentMaterialMap )
			{
				Render.RenderStyle.RenderSurfaces( mRenderCommands, view, item.Value.Span, item.Key, lights );
			}

			// Draw UI etc.
			OnFrameEnd( view, mRenderCommands );
			mRenderCommands.End();
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
			var meshes = meshEntity.Mesh.Submeshes;
			for ( int meshIndex = 0; meshIndex < meshes.Count; meshIndex++ )
			{
				QueueRenderSurface(
					meshes[meshIndex],
					meshEntity.PerEntitySet,
					meshEntity.PerInstanceParameterPools[meshIndex],
					meshEntity.Mesh.Materials[meshIndex] );
			}
		}

		public void QueueRenderSurface( ArrayMesh mesh, ResourceSet set, MaterialParameterPool pool, RenderMaterial material )
		{
			var materialMap = material.Template.Data.PipelineInfo.BlendMode switch
			{
				Blending.Opaque => mOpaqueMaterialMap,
				_               => mTransparentMaterialMap
			};

			var surfaceList = materialMap.GetOrAdd( material, _ => new PooledList<RenderSurface>( 512 ) );
			surfaceList.Add( new()
			{
				Mesh = mesh,
				ParameterPool = pool,
				PerEntitySet = set
			} );
		}

		private void ClearRenderSurfaces()
		{
			foreach ( var surfaceList in mOpaqueMaterialMap.Values )
			{
				surfaceList.Clear();
			}

			foreach ( var surfaceList in mTransparentMaterialMap.Values )
			{
				surfaceList.Clear();
			}
		}
	}
}
