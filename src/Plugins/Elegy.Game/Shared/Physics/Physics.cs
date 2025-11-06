// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Memory;
using Elegy.Common.Maths;
using Elegy.RenderSystem.API;

namespace Game.Shared.Physics
{
	public static partial class Physics
	{
		private static BufferPool mBufferPool;
		private static ThreadDispatcher mThreadDispatcher;

		public static Simulation Simulation { get; private set; }
		public static BroadPhase BroadPhase => Simulation.BroadPhase;
		public static ITimestepper Timestepper => Simulation.Timestepper;

		public static void Init()
		{
			mBufferPool = new( minimumBlockAllocationSize: 131072, expectedPooledResourceCount: 64 );
			mThreadDispatcher = new( Environment.ProcessorCount - 2 );

			Simulation = Simulation.Create( mBufferPool,
				new NarrowphaseCallbacks(),
				new PoseIntegratorCallbacks( gravity: Coords.Down * 9.81f ),
				new SolveDescription( 8, 2, 128 ) );
		}

		public static void Shutdown()
		{
			Simulation.Dispose();
			mBufferPool.Clear();
			mThreadDispatcher.Dispose();
		}

		public static void UpdateSimulation( float deltaTime )
		{
			Simulation.Timestep( deltaTime, mThreadDispatcher );
		}

		public static unsafe void DebugDrawBody( PhysicsBody body )
		{
			Vector3 position = body.IsStatic ? body.PositionStatic : body.Position;
			Quaternion orientation = body.IsStatic ? body.OrientationStatic : body.Orientation;

			int shapeIndex = body.Shape.ShapeIndex.Index;
			int shapeTypeId = body.Shape.ShapeIndex.Type;

			Coords.DirectionsFromQuat( orientation, out var forward, out var up );
			Render.DebugLine( position, position + forward * 2.0f, Vector4.UnitY );
			Render.DebugLine( position, position + up * 2.0f, Vector4.UnitZ );
			Render.DebugLine( position, position + forward.Cross( up ) * 2.0f, Vector4.UnitX );

			if ( shapeTypeId != Mesh.Id )
			{
				// TODO: Port debug draw code from the collision experiment
				return;
			}

			Simulation.Shapes[shapeTypeId].GetShapeData( shapeIndex, out void* data, out _ );
			Mesh* mesh = (Mesh*)data;
			for ( int triangleId = 0; triangleId < mesh->Triangles.Length; triangleId++ )
			{
				Triangle* triangle = (Triangle*)Unsafe.AsPointer( ref mesh->Triangles[triangleId] );
				Vector3 vertexA = Vector3.Transform( triangle->A, orientation );
				Vector3 vertexB = Vector3.Transform( triangle->B, orientation );
				Vector3 vertexC = Vector3.Transform( triangle->C, orientation );

				Render.DebugLine( position + vertexA, position + vertexB, Vector4.UnitX );
				Render.DebugLine( position + vertexB, position + vertexC, Vector4.UnitX );
				Render.DebugLine( position + vertexC, position + vertexA, Vector4.UnitX );
			}
		}
	}
}
