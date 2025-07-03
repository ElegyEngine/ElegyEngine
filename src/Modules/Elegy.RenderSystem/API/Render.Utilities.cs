// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Elegy.Common.Extensions;
using Elegy.RenderBackend.Templating;
using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		private static double mCpuTime;
		private static double mGpuTime;
		private static double mPresentTime;
		private static Stopwatch mStopwatch = new();
		private static double mLastFrameTime;

		private static double GetSeconds() => (double)mStopwatch.ElapsedTicks / Stopwatch.Frequency;

		public static void SetRenderView( CommandList commands, in View view )
		{
			commands.SetFramebuffer( view.RenderBuffer );
			commands.ClearColorTarget( 0, new( 0.01f, 0.05f, 0.06f, 1.0f ) );
			commands.ClearDepthStencil( 1.0f );
			commands.SetViewport( 0, new( 0.0f, 0.0f, view.RenderSize.X, view.RenderSize.Y, 0.0f, 1.0f ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMaterialResourceSetsIndividual(
			CommandList commands, int[] mappings,
			ReadOnlySpan<MaterialParameterSet> parameterSets )
		{
			for ( int i = 0; i < mappings.Length; i++ )
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
					parameterSets[i].ResourceSet! );
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMaterialResourceSets( CommandList commands, RenderMaterial material, ShaderVariant shaderVariant )
		{
			// Set shader parametres used by this shader variant
			// E.g. variant A might not use resource set 2, but variant B might
			// Anything can happen after the first couple builtin sets
			SetMaterialResourceSetsIndividual( commands,
				shaderVariant.ResourceMappingsPerMaterial,
				material.ParameterPool.ParameterSets.AsSpan() );

			// Same story as above, just on a global level
			SetMaterialResourceSetsIndividual( commands,
				shaderVariant.ResourceMappingsGlobal,
				material.GlobalParameterPool.ParameterSets.AsSpan() );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetInstanceResourceSets(
			CommandList commands, MaterialParameterPool perInstancePool, ShaderVariant shaderVariant )
		{
			SetMaterialResourceSetsIndividual( commands,
				shaderVariant.ResourceMappingsPerInstance,
				perInstancePool.ParameterSets.AsSpan() );
		}

		private static RenderBackend.ArrayMesh ViewMesh => Meshes.FullscreenQuad.Submeshes[0];

		private static void RenderViewIntoBackbuffer( in View view )
		{
			var windowDefaultVariant = mWindowMaterial.Template.GetVariant( 0 );
			mRenderCommands.SetPipeline( windowDefaultVariant.Pipeline );
			mRenderCommands.SetFramebuffer( view.BackBuffer );
			mRenderCommands.ClearColorTarget( 0, new( 0.02f, 0.10f, 0.12f, 1.0f ) );
			mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, view.RenderSize.X, view.RenderSize.Y, 0.0f, 1.0f ) );

			mRenderCommands.SetGraphicsResourceSet( 0, view.WindowSet );

			SetMaterialResourceSets( mRenderCommands, mWindowMaterial, windowDefaultVariant );

			mRenderCommands.SetVertexBuffer( 0, ViewMesh.PositionBuffer! );
			mRenderCommands.SetVertexBuffer( 1, ViewMesh.Uv0Buffer! );
			mRenderCommands.SetIndexBuffer( ViewMesh.IndexBuffer!, IndexFormat.UInt32 );

			mRenderCommands.DrawIndexed( ViewMesh.NumIndices );
		}
	}
}
