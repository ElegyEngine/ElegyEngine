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
			List<ResourceSetVariant> resourceVariants, int[] mappings,
			IReadOnlyList<MaterialParameterSet> parameterSets )
		{
			// This contains indices with gaps, e.g. 0, 1, 3, 4, 6
			var shaderVariantSets = resourceVariants[variantIndex].ResourceSetIds;

			for ( int i = 0; i < shaderVariantSets.Length; i++ )
			{
				// This works in a bit of a convoluted way but let me illustrate:
				// Essentially we have 3 different arrays of ResourceSets that represent shader
				// params within a single shader. Let's imagine them on 3 different levels:
				// Sets 0 and 1 are data, sets 2 and 3 are instance and sets 4 and 5 are global.
				// Eventually this will result in (0,sets[0]), (1,sets[1]), (2,sets[2]) etc.
				// So we utilise some remapping tables that take into account all that.
				
				// Let's imagine that the first ID in the remapping table is 2
				// i is 0, and setId is 2
				int setId = mappings[i];

				// Now, because parameterSets also has these same gaps, and is a subset of the indices
				// in shaderVariantSets, we can safely just use i directly here
				commands.SetGraphicsResourceSet(
					(uint)setId,
					parameterSets[i].ResourceSet );
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
				// E.g. variant A might not use resource set 2, but variant B might
				// Anything can happen after the first couple builtin sets
				SetMaterialResources( commands, variantIndex,
					submaterial.ResourceVariants,
					shaderVariant.ResourceMappingsPerMaterial,
					submaterial.ParameterPool.ParameterSets );

				// Same story as above, just on a global level
				SetMaterialResources( commands, variantIndex,
					submaterial.GlobalResourceVariants,
					shaderVariant.ResourceMappingsGlobal,
					submaterial.GlobalParameterPool.ParameterSets );

				// Same story as above, just on an instance level
				var parameterPool = entity.PerInstanceParameterPools[i];
				SetMaterialResources( commands, variantIndex,
					parameterPool.ResourceSetVariants,
					shaderVariant.ResourceMappingsPerInstance,
					parameterPool.ParameterSets );

				// Send vertex buffers used by this shader variant
				ReadOnlySpan<VariantVertexAttribute> vertexAttributes = shaderVariant.VertexAttributes;
				for ( int vertexAttributeId = 0; vertexAttributeId < vertexAttributes.Length; vertexAttributeId++ )
				{
					var vertexAttribute = vertexAttributes[vertexAttributeId];
					var buffer = submesh.GetBuffer( vertexAttribute.Semantic, vertexAttribute.Channel );

					Debug.Assert( buffer is not null, $"The '{vertexAttribute.Semantic}' buffer is MISSING" );

					commands.SetVertexBuffer( (uint)vertexAttributeId, buffer );
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
