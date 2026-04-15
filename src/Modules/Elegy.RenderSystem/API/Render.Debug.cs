// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
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
		[StructLayout( LayoutKind.Sequential )]
		private struct DebugLineEntry
		{
			public Vector3 Start;
			public Vector3 End;
			public Vector4B Colour;
			public float Thickness;
		}

		private const int MaxDebugLines = 8192;
		private static int mDebugLinesWorldAdded;
		private static int mDebugLinesScreenAdded;
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
					Uv0 = new Vector2[MaxDebugLines * 4],
					Indices = new uint[MaxDebugLines * 6]
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
			mDebugLinesWorldAdded = 0;
			mDebugLinesScreenAdded = 0;

			mDebugLineMesh = CreateMesh( mDebugLinesWorldModel );
		}

		private static void RebuildDebugMeshes( CommandList cmd )
		{
			var mesh = mDebugLinesWorldModel.Meshes[0];

			var span = mDebugLinesWorld.AsSpan();
			for ( int lineIndex = 0; lineIndex < mDebugLinesWorldAdded; lineIndex++ )
			{
				ref var line = ref span[lineIndex];

				Vector3 direction = (line.End - line.Start).Normalized();
				int vertexIndex = lineIndex * 4;
				int indexIndex = lineIndex * 6;

				mesh.Positions[vertexIndex] = line.Start;
				mesh.Positions[vertexIndex + 1] = line.Start;
				mesh.Positions[vertexIndex + 2] = line.End;
				mesh.Positions[vertexIndex + 3] = line.End;

				mesh.Normals[vertexIndex] = direction;
				mesh.Normals[vertexIndex + 1] = direction;
				mesh.Normals[vertexIndex + 2] = direction;
				mesh.Normals[vertexIndex + 3] = direction;

				// Each "line" is actually a thin quad in disguise, and this
				// UV component is used to figure out left/right and thickness.
				mesh.Uv0[vertexIndex] = new( -1.0f, line.Thickness );
				mesh.Uv0[vertexIndex + 1] = new( 1.0f, line.Thickness );
				mesh.Uv0[vertexIndex + 2] = new( 1.0f, line.Thickness );
				mesh.Uv0[vertexIndex + 3] = new( -1.0f, line.Thickness );

				mesh.Color0[vertexIndex] = line.Colour;
				mesh.Color0[vertexIndex + 1] = line.Colour;
				mesh.Color0[vertexIndex + 2] = line.Colour;
				mesh.Color0[vertexIndex + 3] = line.Colour;

				mesh.Indices[indexIndex] = (uint)vertexIndex;
				mesh.Indices[indexIndex + 1] = (uint)vertexIndex + 1;
				mesh.Indices[indexIndex + 2] = (uint)vertexIndex + 2;
				mesh.Indices[indexIndex + 3] = (uint)vertexIndex;
				mesh.Indices[indexIndex + 4] = (uint)vertexIndex + 2;
				mesh.Indices[indexIndex + 5] = (uint)vertexIndex + 3;
			}

			mDebugLinesWorldActive = mDebugLinesWorldAdded is not 0;
			if ( mDebugLinesWorldActive )
			{
				DebugLineMesh.UpdateDynamic( mesh, mDebugLinesWorldAdded * 4, mDebugLinesWorldAdded * 6, cmd );
			}

			mDebugLinesWorldAdded = 0;
			mDebugLinesScreenAdded = 0;
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

			var worldVariant = mDebugLineMaterial.Template.ShaderVariants["GENERAL"];

			mRenderCommands.SetFramebuffer( view.RenderBuffer );
			mRenderCommands.SetViewport( 0, new( 0.0f, 0.0f, view.RenderSize.X, view.RenderSize.Y, 0.0f, 1.0f ) );

			mRenderCommands.SetPipeline( worldVariant.Pipeline );
			mRenderCommands.SetGraphicsResourceSet( 0, view.PerViewSet );

			SetMaterialResourceSets( mRenderCommands, mDebugLineMaterial, worldVariant );

			mRenderCommands.SetVertexBuffer( 0, DebugLineMesh.PositionBuffer );
			mRenderCommands.SetVertexBuffer( 1, DebugLineMesh.NormalBuffer );
			mRenderCommands.SetVertexBuffer( 2, DebugLineMesh.Uv0Buffer );
			mRenderCommands.SetVertexBuffer( 3, DebugLineMesh.Color0Buffer );
			mRenderCommands.SetIndexBuffer( DebugLineMesh.IndexBuffer, IndexFormat.UInt32 );

			mRenderCommands.DrawIndexed( DebugLineMesh.NumIndices );
		}

		private static void SubmitLine( ref int count, DebugLineEntry[] list, in DebugLineEntry entry )
		{
			if ( count >= list.Length )
			{
				return;
			}

			list[count] = entry;
			count++;
		}

		#region 3D debug primitives

		/// <summary>
		/// Submits a 3D debug line.
		/// </summary>
		public static void DebugLine( Vector3 start, Vector3 end, float thickness = 1.0f )
			=> SubmitLine( ref mDebugLinesWorldAdded, mDebugLinesWorld, new()
			{
				Start = start,
				End = end,
				Colour = (Vector4B)(Vector4.One * 255.0f),
				Thickness = thickness
			} );

		/// <summary>
		/// Submits a 3D debug line.
		/// </summary>
		public static void DebugLine( Vector3 start, Vector3 end, Vector4 colour, float thickness = 1.0f )
			=> SubmitLine(  ref mDebugLinesWorldAdded, mDebugLinesWorld, new()
			{
				Start = start,
				End = end,
				Colour = (Vector4B)(colour * 255.0f),
				Thickness = thickness
			} );

		/// <summary>
		/// Submits a 3D debug box.
		/// </summary>
		public static void DebugBox( Vector3 position, Vector3 extents, Vector4 colour )
			=> DebugBoxEx( position, Coords.Forward, Coords.Up, extents, colour );

		/// <summary>
		/// Submits a 3D debug box with orientation.
		/// </summary>
		public static void DebugBoxEx( Vector3 position, Vector3 forward, Vector3 up, Vector3 extents, Vector4 colour )
		{
			Vector3 right = Vector3.Cross( forward, up ) * extents.X;
			forward *= extents.Y;
			up *= extents.Z;

			// Top square
			DebugLine( position + forward + right + up, position + forward - right + up, colour );
			DebugLine( position - forward + right + up, position - forward - right + up, colour );
			DebugLine( position + forward + right + up, position - forward + right + up, colour );
			DebugLine( position + forward - right + up, position - forward - right + up, colour );
			// Bottom square
			DebugLine( position + forward + right - up, position + forward - right - up, colour );
			DebugLine( position - forward + right - up, position - forward - right - up, colour );
			DebugLine( position + forward + right - up, position - forward + right - up, colour );
			DebugLine( position + forward - right - up, position - forward - right - up, colour );
			// 4 corner pillars
			DebugLine( position + forward + right + up, position + forward + right - up, colour );
			DebugLine( position + forward - right + up, position + forward - right - up, colour );
			DebugLine( position - forward - right + up, position - forward - right - up, colour );
			DebugLine( position - forward + right + up, position - forward + right - up, colour );
		}

		/// <summary>
		/// Submits a 3D debug capsule.
		/// </summary>
		public static void DebugCapsule( Vector3 position, Vector3 forward, Vector3 up, float halfHeight, float radius, Vector4 colour )
		{
			var topSphere = position + up * halfHeight;
			var bottomSphere = position - up * halfHeight;

			DebugCylinder( position, forward, up, halfHeight, radius, colour );
			DebugSphereEx( topSphere, forward, up, radius, colour, keepSide: 1 );
			DebugSphereEx( bottomSphere, forward, up, radius, colour, keepSide: -1 );
		}

		/// <summary>
		/// Submits a 3D debug circle.
		/// </summary>
		public static void DebugDrawCircle( Vector3 position, Vector3 poleAxis, Vector3 equatorAxis, float radius, Vector4 colour,
			int sidesPerQuarter = 2, int keepY = 0 )
		{
			// Create a quarter of a circle, scaled by radius
			Span<Vector2> quarterPoints = stackalloc Vector2[sidesPerQuarter];
			quarterPoints[0] = new( radius, 0.0f );
			for ( int i = 1; i < sidesPerQuarter; i++ )
			{
				// Good old polar coordinates
				float t = (float)i / sidesPerQuarter;
				(float y, float x) = MathF.SinCos( t * MathF.PI * 0.5f );
				quarterPoints[i] = new( x * radius, y * radius );
			}

			// Then construct a full circle by repeating that quarter in different quadrants,
			// simultaneously transforming it into (potentially rotated) 3D space
			Span<Vector3> points = stackalloc Vector3[sidesPerQuarter * 4];
			for ( int i = 0; i < sidesPerQuarter * 4; i++ )
			{
				int quadrantId = i / sidesPerQuarter;
				Vector2 p = quarterPoints[i % sidesPerQuarter];
				switch ( quadrantId )
				{
					case 1: p = new( -p.Y, p.X ); break; // (1,2) into (-2,1)
					case 2: p = new( -p.X, -p.Y ); break; // (1,2) into (-1,-2)
					case 3: p = new( p.Y, -p.X ); break; // (1,2) into (2,-1)
				}

				points[i] = p.X * equatorAxis + p.Y * poleAxis;
			}

			int startId = keepY >= 0 ? 0 : points.Length / 2;
			int endId = keepY > 0 ? points.Length / 2 : points.Length;

			for ( int i = startId; i < endId; i++ )
			{
				int j = (i + 1) % points.Length;
				DebugLine( position + points[i], position + points[j], colour );
			}
		}

		/// <summary>
		/// Submits a 3D debug cylinder.
		/// </summary>
		public static void DebugCylinder( Vector3 position, Vector3 forward, Vector3 up, float halfHeight, float radius,
			Vector4 colour )
		{
			Vector3 right = Vector3.Cross( forward, up );
			Vector3 capTop = position + up * halfHeight;
			Vector3 capBottom = position - up * halfHeight;

			// Caps
			DebugDrawCircle( capTop, forward, right, radius, colour, 3 );
			DebugDrawCircle( capBottom, forward, right, radius, colour, 3 );

			// Premultiply the axes
			up *= halfHeight;
			forward *= radius;
			right *= radius;

			// 4 pillars, front, back, right, left
			DebugLine( position + forward + up, position + forward - up, colour );
			DebugLine( position - forward + up, position - forward - up, colour );
			DebugLine( position + right + up, position + right - up, colour );
			DebugLine( position - right + up, position - right - up, colour );
		}

		/// <summary>
		/// Submits a 3D debug sphere.
		/// </summary>
		public static void DebugSphere( Vector3 position, float radius, Vector4 colour )
			=> DebugSphereEx( position, Coords.Forward, Coords.Up, radius, colour );

		/// <summary>
		/// Submits a 3D debug sphere with orientation.
		/// </summary>
		public static void DebugSphereEx( Vector3 position, Vector3 forward, Vector3 up, float radius, Vector4 colour,
			int keepSide = 0 )
		{
			// keepSide is used to help rendering capsules:
			// -1 -> keep bottom hemisphere
			// 0 -> keep both
			// 1 -> keep top hemisphere
			var right = Vector3.Cross( forward, up );
			DebugDrawCircle( position, up, forward, radius, colour, 4, keepSide );
			DebugDrawCircle( position, up, right, radius, colour, 4, keepSide );
		}

		#endregion

		/// <summary>
		/// Submits a 2D debug line for debug rendering.
		/// </summary>
		public static void DebugLineScreen( Vector2 start, Vector2 end, float thickness = 1.0f )
			=> SubmitLine( ref mDebugLinesScreenAdded, mDebugLinesScreen, new()
			{
				Start = new Vector3( start, 0.0f ),
				End = new Vector3( end, 0.0f ),
				Colour = (Vector4B)(Vector4.One * 255.0f),
				Thickness = thickness
			} );

		/// <summary>
		/// Submits a 2D debug line for debug rendering.
		/// </summary>
		public static void DebugLineScreen(
			Vector2 start, Vector2 end, Vector4 colour, float thickness = 1.0f )
			=> SubmitLine( ref mDebugLinesScreenAdded, mDebugLinesScreen, new()
			{
				Start = new Vector3( start, 0.0f ),
				End = new Vector3( end, 0.0f ),
				Colour = (Vector4B)(colour * 255.0f),
				Thickness = thickness
			} );
	}
}
