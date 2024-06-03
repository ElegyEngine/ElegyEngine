// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;
using Elegy.RenderSystem.API;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Veldrid;

namespace Elegy.RenderSystem
{
	internal class RenderStyleDefault : IRenderStyle
	{
		private TaggedLogger mLogger = new( "RSDefault" );

		public string[] InstanceExtensions => [];

		public string[] DeviceExtensions => ["VK_KHR_fragment_shader_barycentric"];

		public string Name => "RenderStyleDefault";

		public string Error => string.Empty;

		public bool Initialised { get; private set; } = false;

		public bool Init()
		{
			mLogger.Log( "Init" );
			Initialised = true;
			return true;
		}

		public void Shutdown()
		{
			mLogger.Log( "Shutdown" );
		}

		public bool CreateCorePipelines()
		{
			return true;
		}

		public void RenderBatches( CommandList renderCommand, View view, ReadOnlySpan<Batch> billboards, ReadOnlySpan<Light> lights )
		{

		}

		public void RenderBillboards( CommandList renderCommand, View view, ReadOnlySpan<Billboard> billboards, ReadOnlySpan<Light> lights )
		{

		}

		public void RenderSurfaces( CommandList renderCommand, View view,
			ReadOnlySpan<RenderSurface> surfaces, RenderMaterial material, ReadOnlySpan<Light> lights )
		{
			int variantIndex = material.ParameterPool.GetVariantIndex( "GENERAL" );
			var shaderVariant = material.Template.ShaderVariants.ElementAt( variantIndex ).Value;
			
			renderCommand.SetPipeline( shaderVariant.Pipeline );
			renderCommand.SetGraphicsResourceSet( 0, view.PerViewSet );

			for ( int i = 0; i < surfaces.Length; i++ )
			{
				var surface = surfaces[i];

				// We have a few hardcoded resource set IDs
				// 0 is always per-frame/per-view data (all about the camera basically)
				// 1 is always per-entity data (entity transform matrix, bone matrices etc.)
				renderCommand.SetGraphicsResourceSet( 1, surface.PerEntitySet );

				// Set shader parametres used by this shader variant
				Render.SetMaterialResourceSets( renderCommand, material, variantIndex, surface.ParameterPool );

				// Send vertex buffers used by this shader variant
				foreach ( var vertexAttribute in shaderVariant.VertexAttributes.AsSpan() )
				{
					var buffer = surface.Mesh.GetBuffer( vertexAttribute.Semantic, vertexAttribute.Channel );
					renderCommand.SetVertexBuffer( vertexAttribute.Id, buffer );
				}

				// AT LAST, render the damn thing
				renderCommand.SetIndexBuffer( surface.Mesh.IndexBuffer, IndexFormat.UInt32 );
				renderCommand.DrawIndexed( surface.Mesh.NumIndices );
			}
		}

		public void RenderVolume( CommandList renderCommand, View view, Volume volume, ReadOnlySpan<Light> lights )
		{

		}
	}
}
