// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ConsoleSystem;
using Elegy.RenderBackend.Templating;
using Elegy.RenderSystem.Interfaces;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetMaterialResources( CommandList commands, int variantIndex,
			List<ResourceSetVariant> resourceVariants, VariantResourceMapping[] mappings )
		{
			var shaderVariantSets = resourceVariants[variantIndex].ResourceSets;
			for ( int resourceSetId = 0; resourceSetId < shaderVariantSets.Length; resourceSetId++ )
			{
				commands.SetGraphicsResourceSet(
					// Maintain correct slot
					(uint)mappings[resourceSetId].SetId,
					// Each shader variant provides its own copy of resource sets
					// It's a bit wasteful but was simpler to implement
					shaderVariantSets[resourceSetId] );
			}
		}

		private static void RenderSingleEntity( CommandList commands, MeshEntity entity, View view )
		{
			for ( int i = 0; i < entity.Mesh.Submeshes.Count; i++ )
			{
				// TODO: sort render ents by material
				var submesh = entity.Mesh.Submeshes[i];
				var submaterial = entity.Mesh.Materials[i];
				int variantIndex = submaterial.ParameterPool.GetVariantIndex( "GENERAL" );
				var shaderVariant = submaterial.Template.ShaderVariants.ElementAt( variantIndex ).Value;

				commands.SetPipeline( shaderVariant.Pipeline );

				// We have a few hardcoded resource set IDs
				// 0 is always per-frame/per-view data (all about the camera basically)
				// 1 is always per-entity data (entity transform matrix, bone matrices etc.)
				commands.SetGraphicsResourceSet( 0, view.PerViewSet );
				commands.SetGraphicsResourceSet( 1, entity.PerEntitySet );

				// Set shader parametres used by this shader variant
				// E.g. variant A might not use certain buffers so it omits an entire resource set,
				// while variant B might have them in a different order. Anything can happen after
				// the first few hardcoded sets 0 and 1
				SetMaterialResources( commands, variantIndex,
					submaterial.ResourceVariants, shaderVariant.ResourceMappingsPerMaterial );

				// Same story as above, just on a global level
				SetMaterialResources( commands, variantIndex,
					submaterial.GlobalResourceVariants, shaderVariant.ResourceMappingsGlobal );

				// Same story as above, just on an instance level
				var parameterPool = entity.PerInstanceParameterPools[i];
				SetMaterialResources( commands, variantIndex,
					parameterPool.ResourceSetVariants, shaderVariant.ResourceMappingsPerInstance );

				// Send vertex buffers used by this shader variant
				// I wonder if this would get any faster using 'unsafe' or Span<T>, but
				// frankly this is not yet at a stage where it's worth profiling much
				for ( int vertexAttributeId = 0; vertexAttributeId < shaderVariant.VertexAttributes.Length; vertexAttributeId++ )
				{
					var vertexAttribute = shaderVariant.VertexAttributes[vertexAttributeId];
					var buffer = submesh.GetBuffer( vertexAttribute.Semantic, vertexAttribute.Channel );

					Debug.Assert( buffer is not null, $"The '{vertexAttribute.Semantic}' buffer is MISSING" );

					commands.SetVertexBuffer(
						(uint)vertexAttributeId,
						buffer );
				}

				// AT LAST, render the damn thing
				commands.SetIndexBuffer( submesh.IndexBuffer, IndexFormat.UInt32 );
				commands.DrawIndexed( submesh.NumIndices );
			}
		}

		public void RenderMeshEntities( CommandList renderCommand, View view, ReadOnlySpan<MeshEntity> entities, ReadOnlySpan<Light> lights )
		{
			foreach ( var entity in entities )
			{
				RenderSingleEntity( renderCommand, entity, view );
			}
		}

		public void RenderVolume( CommandList renderCommand, View view, Volume volume, ReadOnlySpan<Light> lights )
		{

		}
	}
}
