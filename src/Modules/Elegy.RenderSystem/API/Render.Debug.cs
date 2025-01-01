// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Numerics;
using Elegy.Common.Assets;
using Elegy.Common.Extensions;
using Elegy.Common.Maths;
using Elegy.RenderBackend;
using Elegy.RenderSystem.Objects;
using Elegy.RenderSystem.Resources;
using Veldrid;

namespace Elegy.RenderSystem.API
{
	public static partial class Render
	{
		private struct DebugLineEntry
		{
			public float ExpireTime;
			public Vector3 Start;
			public Vector3 End;
			public Vector4B Colour;
			public float Thickness;
		}

		private const int MaxDebugLines = 8192;
		private static DebugLineEntry[] mDebugLinesWorld = new DebugLineEntry[MaxDebugLines];
		private static DebugLineEntry[] mDebugLinesScreen = new DebugLineEntry[MaxDebugLines];
		private static bool mDebugLinesWorldActive;
		private static bool mDebugLinesScreenActive;

		private static Model mDebugLinesWorldModel = new()
		{
			Name = "DebugLinesWorld",
			FullPath = "__debug_lines_world",
			Meshes =
			[
				new()
				{
					Name = "DebugLinesWorld Mesh",
					MaterialName = "materials/builtin/debug_line",
					NumDynamicVertices = 0,
					NumDynamicIndices = 0,
					Positions = new Vector3[MaxDebugLines * 4],
					// TODO: per-instance vertex stepping, so we can cut 3/4 of the normals and 1/2 of the colours here
					Normals = new Vector3[MaxDebugLines * 4],
					Color0 = new Vector4B[MaxDebugLines * 4],
					Uv0 = new Vector2[MaxDebugLines     * 4],
					Indices = new uint[MaxDebugLines    * 6]
				}
			]
		};

		private static Model mDebugLinesScreenModel = new();

		private static RenderMaterial mDebugLineMaterial;
		private static Mesh mDebugLineMesh;
		private static Mesh mDebugLineMeshScreen;
		private static ArrayMesh DebugLineMesh => mDebugLineMesh.Submeshes[0];
		private static ArrayMesh DebugLineScreenMesh => mDebugLineMesh.Submeshes[0];

		private static void InitialiseDebugMeshes()
		{
			var span = mDebugLinesWorld.AsSpan();
			for ( int i = 0; i < span.Length; i++ )
			{
				span[i].ExpireTime = -1.0f;
			}

			mDebugLineMesh = CreateMesh( mDebugLinesWorldModel );
		}

		private static void RebuildDebugMeshes()
		{
			var mesh = mDebugLinesWorldModel.Meshes[0];

			int lineIndex = 0;
			var span = mDebugLinesWorld;
			for ( int i = 0; i < span.Length; i++ )
			{
				if ( span[i].ExpireTime < mLastFrameTime )
				{
					// Expiry times that are equal to float.MinValue are basically
					// "this frame only, delete me next frame thanks"
					if ( span[i].ExpireTime != float.MinValue )
					{
						continue;
					}

					span[i].ExpireTime = -1.0f;
				}

				Vector3 direction = (span[i].End - span[i].Start).Normalized();
				int vertexIndex = lineIndex * 4;
				int indexIndex = lineIndex  * 6;

				mesh.Positions[vertexIndex] = span[i].Start;
				mesh.Positions[vertexIndex + 1] = span[i].Start;
				mesh.Positions[vertexIndex + 2] = span[i].End;
				mesh.Positions[vertexIndex + 3] = span[i].End;

				mesh.Normals[vertexIndex] = direction;
				mesh.Normals[vertexIndex + 1] = direction;
				mesh.Normals[vertexIndex + 2] = direction;
				mesh.Normals[vertexIndex + 3] = direction;

				// Each "line" is actually a thin quad in disguise, and this
				// UV component is used to figure out left/right and thickness.
				mesh.Uv0[vertexIndex] = new( -1.0f, span[i].Thickness );
				mesh.Uv0[vertexIndex + 1] = new( 1.0f, span[i].Thickness );
				mesh.Uv0[vertexIndex + 2] = new( 1.0f, span[i].Thickness );
				mesh.Uv0[vertexIndex + 3] = new( -1.0f, span[i].Thickness );

				mesh.Color0[vertexIndex] = span[i].Colour;
				mesh.Color0[vertexIndex + 1] = span[i].Colour;
				mesh.Color0[vertexIndex + 2] = span[i].Colour;
				mesh.Color0[vertexIndex + 3] = span[i].Colour;

				mesh.Indices[indexIndex] = (uint)vertexIndex;
				mesh.Indices[indexIndex + 1] = (uint)vertexIndex + 1;
				mesh.Indices[indexIndex + 2] = (uint)vertexIndex + 2;
				mesh.Indices[indexIndex + 3] = (uint)vertexIndex;
				mesh.Indices[indexIndex + 4] = (uint)vertexIndex + 2;
				mesh.Indices[indexIndex + 5] = (uint)vertexIndex + 3;

				lineIndex++;
			}

			mDebugLinesWorldActive = lineIndex is not 0;
			if ( mDebugLinesWorldActive )
			{
				DebugLineMesh.UpdateDynamic( mesh, lineIndex * 4, lineIndex * 6 );
			}
		}

		private static void RenderDebugLines( in View view )
		{
			if ( !mDebugLinesWorldActive )
			{
				return;
			}

			Debug.Assert( DebugLineMesh.PositionBuffer is not null );
			Debug.Assert( DebugLineMesh.NormalBuffer is not null );
			Debug.Assert( DebugLineMesh.Uv0Buffer is not null );
			Debug.Assert( DebugLineMesh.Color0Buffer is not null );
			Debug.Assert( DebugLineMesh.IndexBuffer is not null );

			var worldVariantIndex = mDebugLineMaterial.ParameterPool.GetVariantIndex( "GENERAL" );
			var worldVariant = mDebugLineMaterial.Template.GetVariant( worldVariantIndex );

			mRenderCommands.SetFramebuffer( view.RenderFramebuffer );
			mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, view.RenderSize.X, view.RenderSize.Y, 0.0f, 1.0f ) );

			mRenderCommands.SetPipeline( worldVariant.Pipeline );
			mRenderCommands.SetGraphicsResourceSet( 0, view.PerViewSet );

			SetMaterialResourceSets( mRenderCommands, mDebugLineMaterial, worldVariantIndex );

			mRenderCommands.SetVertexBuffer( 0, DebugLineMesh.PositionBuffer );
			mRenderCommands.SetVertexBuffer( 1, DebugLineMesh.NormalBuffer );
			mRenderCommands.SetVertexBuffer( 2, DebugLineMesh.Uv0Buffer );
			mRenderCommands.SetVertexBuffer( 3, DebugLineMesh.Color0Buffer );
			mRenderCommands.SetIndexBuffer( DebugLineMesh.IndexBuffer, IndexFormat.UInt32 );

			mRenderCommands.DrawIndexed( DebugLineMesh.NumIndices );
		}

		private static void SubmitLine( DebugLineEntry[] list, in DebugLineEntry entry )
		{
			if ( entry.Start.IsEqualApprox( entry.End, 0.01f ) )
			{
				return;
			}

			float lastFrameTime = (float)mLastFrameTime;

			int oldestIndex = 0;
			float oldestExpireTime = float.MaxValue;

			// Look for any expired debug lines and insert there
			// Otherwise, grab the oldest line and replace it with that
			for ( int i = 0; i < list.Length; ++i )
			{
				float expireTime = list[i].ExpireTime;

				// Single-frame entries are avoided
				if ( expireTime == float.MinValue )
				{
					continue;
				}

				if ( expireTime < lastFrameTime )
				{
					list[i] = entry;
					return;
				}

				if ( expireTime < oldestExpireTime )
				{
					oldestExpireTime = expireTime;
					oldestIndex = i;
				}
			}

			list[oldestIndex] = entry;
		}

		/// <summary>
		/// Submits a 3D debug line for debug rendering.
		/// </summary>
		public static void DebugLine( Vector3 start, Vector3 end, float lifetime = float.MinValue, float thickness = 1.0f )
			=> SubmitLine( mDebugLinesWorld, new()
			{
				ExpireTime = (float)mLastFrameTime + lifetime,
				Start = start,
				End = end,
				Colour = (Vector4B)(Vector4.One * 255.0f),
				Thickness = thickness
			} );

		/// <summary>
		/// Submits a 3D debug line for debug rendering.
		/// </summary>
		public static void DebugLine( Vector3 start, Vector3 end, Vector4 colour, float lifetime = float.MinValue, float thickness = 1.0f )
			=> SubmitLine( mDebugLinesWorld, new()
			{
				ExpireTime = (float)mLastFrameTime + lifetime,
				Start = start,
				End = end,
				Colour = (Vector4B)(colour * 255.0f),
				Thickness = thickness
			} );

		/// <summary>
		/// Submits a 2D debug line for debug rendering.
		/// </summary>
		public static void DebugLineScreen( Vector2 start, Vector2 end, float lifetime = float.MinValue, float thickness = 1.0f )
			=> SubmitLine( mDebugLinesScreen, new()
			{
				ExpireTime = (float)mLastFrameTime + lifetime,
				Start = new Vector3( start, 0.0f ),
				End = new Vector3( end, 0.0f ),
				Colour = (Vector4B)(Vector4.One * 255.0f),
				Thickness = thickness
			} );

		/// <summary>
		/// Submits a 2D debug line for debug rendering.
		/// </summary>
		public static void DebugLineScreen(
			Vector2 start, Vector2 end, Vector4 colour, float lifetime = float.MinValue, float thickness = 1.0f )
			=> SubmitLine( mDebugLinesScreen, new()
			{
				ExpireTime = (float)mLastFrameTime + lifetime,
				Start = new Vector3( start, 0.0f ),
				End = new Vector3( end, 0.0f ),
				Colour = (Vector4B)(colour * 255.0f),
				Thickness = thickness
			} );
	}
}
