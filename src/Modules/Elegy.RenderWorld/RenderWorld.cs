// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Collections.Pooled;
using Elegy.PlatformSystem.API;
using Elegy.RenderBackend;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Veldrid;

using Blending = Elegy.RenderBackend.Assets.Blending;

namespace Elegy.RenderWorld
{
	public partial class RenderWorld
	{
		private CommandList mRenderCommands;

		public PooledDictionary<RenderMaterial, PooledList<RenderSurface>> mOpaqueMaterialMap = new( capacity: 512 );
		public PooledDictionary<RenderMaterial, PooledList<RenderSurface>> mTransparentMaterialMap = new( capacity: 512 );

		public RenderWorld()
		{
			mRenderCommands = Render.Factory.CreateCommandList();
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

			// Gather all render surfaces
			// It is possible to cache these for static props per map area, and
			// regenerate dynamic ones instead this way. Also, occlusion culling
			ClearRenderSurfaces();
			foreach ( var entity in MeshEntities )
			{
				for ( int i = 0; i < entity.Mesh.Submeshes.Count; i++ )
				{
					QueueRenderSurface(
						entity.Mesh.Submeshes[i],
						entity.PerEntitySet,
						entity.PerInstanceParameterPools[i],
						entity.Mesh.Materials[i] );
				}
			}

			// Currently we have no lights, so oops
			ReadOnlySpan<Light> lights = [];

			// Update all buffers at the start of the frame pretty please
			Render.UpdateBuffers();

			// Begin rendering
			mRenderCommands.Begin();
			Render.SetRenderView( mRenderCommands, currentView );

			// Opaque pass
			foreach ( var item in mOpaqueMaterialMap )
			{
				Render.RenderStyle.RenderSurfaces( mRenderCommands, currentView, item.Value.Span, item.Key, lights );
			}

			// Transparent pass
			foreach ( var item in mTransparentMaterialMap )
			{
				Render.RenderStyle.RenderSurfaces( mRenderCommands, currentView, item.Value.Span, item.Key, lights );
			}

			mRenderCommands.End();
			Render.Device.SubmitCommands( mRenderCommands );
		}

		private void QueueRenderSurface( ArrayMesh mesh, ResourceSet set, MaterialParameterPool pool, RenderMaterial material )
		{
			var materialMap = material.Template.Data.PipelineInfo.BlendMode switch
			{
				Blending.Opaque => mOpaqueMaterialMap,
				_ => mTransparentMaterialMap
			};

			var surfaceList = materialMap.GetOrAdd( material, renderMaterial => new PooledList<RenderSurface>( 512 ) );
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
