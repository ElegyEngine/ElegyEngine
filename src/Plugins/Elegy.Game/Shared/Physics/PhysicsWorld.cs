// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Elegy.Common.Maths;
using Elegy.RenderSystem.API;
using Game.Shared.Components;

namespace Game.Shared.Physics
{
	public static class PhysicsWorld
	{
		private static BufferPool mBufferPool;
		private static ThreadDispatcher mThreadDispatcher;

		public static Simulation Simulation { get; private set; }

		public static void Init()
		{
			mBufferPool = new( minimumBlockAllocationSize: 131072, expectedPooledResourceCount: 64 );
			mThreadDispatcher = new( Environment.ProcessorCount );

			Simulation = Simulation.Create( mBufferPool,
				new NarrowphaseCallbacks(),
				new PoseIntegratorCallbacks( gravity: Coords.Down * 9.81f ),
				new SolveDescription( 8, 1, 512 ) );
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

		public static PhysicsBody CreateBody( in Transform worldTransform, PhysicsShape shape )
		{
			bool needsConversion = shape.ShapeIndex.Type != Mesh.Id;
			Quaternion orientation = needsConversion ? PhysicsBody.ToBepu( worldTransform.Orientation ) : worldTransform.Orientation;
			
			PhysicsBody result = new(
				shape: shape,
				dynamicHandle: Simulation.Bodies.Add( BodyDescription.CreateDynamic(
					pose: new( worldTransform.Position, orientation ),
					inertia: shape.Inertia,
					collidable: shape.ShapeIndex,
					activity: 0.01f ) ),
				needsConversion: needsConversion );

			return result;
		}

		public static PhysicsBody CreateKinematicBody( in Transform worldTransform, PhysicsShape shape )
		{
			bool needsConversion = shape.ShapeIndex.Type != Mesh.Id;
			Quaternion orientation = needsConversion ? PhysicsBody.ToBepu( worldTransform.Orientation ) : worldTransform.Orientation;

			PhysicsBody result = new(
				shape: shape,
				dynamicHandle: Simulation.Bodies.Add( BodyDescription.CreateDynamic(
					pose: new( worldTransform.Position, orientation ),
					// This type of inertia prevents unwanted rotations
					inertia: new() { InverseMass = shape.Inertia.InverseMass },
					collidable: new( shape.ShapeIndex, 0.1f, float.MaxValue, ContinuousDetection.Passive ),
					activity: 0.02f ) ),
				needsConversion: needsConversion );

			return result;
		}

		public static PhysicsBody CreateStaticBody( in Transform worldTransform, PhysicsShape shape )
		{
			PhysicsBody result = new(
				shape: shape,
				staticHandle: Simulation.Statics.Add( new(
					pose: new( worldTransform.Position, worldTransform.Orientation ),
					shape: shape.ShapeIndex,
					continuity: ContinuousDetection.Discrete ) ) );

			return result;
		}

		public static PhysicsShape CreateShape<TShape>( TShape shape, float mass = 1.0f )
			where TShape : unmanaged, IConvexShape
		{
			PhysicsShape result = new()
			{
				ShapeIndex = Simulation.Shapes.Add( shape ),
				Inertia = shape.ComputeInertia( mass )
			};

			return result;
		}

		public static PhysicsShape CreateMeshShape( Buffer<Triangle> triangles, float mass = -1.0f )
		{
			Mesh meshShape = new( triangles, Vector3.One, mBufferPool );

			PhysicsShape result = new()
			{
				ShapeIndex = Simulation.Shapes.Add( meshShape ),
				Inertia = mass < 0.0f
					? meshShape.ComputeOpenInertia( -mass )
					: meshShape.ComputeClosedInertia( mass )
			};

			return result;
		}

		public static PhysicsShape CreateMeshShape( Elegy.Common.Assets.ElegyMapData.CollisionMesh collisionMesh, float mass = -1.0f )
		{
			int triangleIndex = 0;
			int numTriangles = 0;
			foreach ( var meshlet in collisionMesh.Meshlets )
			{
				numTriangles += meshlet.Positions.Count / 3;
			}

			mBufferPool.Take<Triangle>( numTriangles, out var triangles );

			foreach ( var meshlet in collisionMesh.Meshlets )
			{
				for ( int i = 0; i < meshlet.Positions.Count; i += 3 )
				{
					triangles[triangleIndex] = new()
					{
						A = meshlet.Positions[i + 2],
						B = meshlet.Positions[i + 1],
						C = meshlet.Positions[i + 0]
					};
				}

				triangleIndex++;
			}

			return CreateMeshShape( triangles, mass );
		}

		public static PhysicsShape CreateMeshShape( Elegy.Common.Assets.Model model, float mass = -1.0f )
		{
			// TEMP until meshes work again
			//PhysicsShape result = new()
			//{
			//	ShapeIndex = Simulation.Shapes.Add( new Box( 20.0f, 20.0f, 0.5f ) ),
			//	Inertia = new()
			//};
			//return result;

			int triangleIndex = 0;
			int numTriangles = 0;
			foreach ( var mesh in model.Meshes )
			{
				numTriangles += mesh.Indices.Length / 3;
			}

			mBufferPool.Take<Triangle>( numTriangles, out var triangles );

			foreach ( var mesh in model.Meshes )
			{
				for ( int i = 0; i < mesh.Indices.Length; i += 3 )
				{
					triangles[triangleIndex] = new()
					{
						A = mesh.Positions[mesh.Indices[i + 2]],
						B = mesh.Positions[mesh.Indices[i + 1]],
						C = mesh.Positions[mesh.Indices[i + 0]]
					};

					triangleIndex++;
				}
			}

			return CreateMeshShape( triangles, mass );
		}

		public static unsafe void DebugDrawBody( PhysicsBody body )
		{
			Vector3 position = body.IsStatic ? body.PositionStatic : body.Position;
			Quaternion orientation = body.IsStatic ? body.OrientationStatic : body.Orientation;

			int shapeIndex = body.Shape.ShapeIndex.Index;
			int shapeTypeId = body.Shape.ShapeIndex.Type;

			Coords.DirectionsFromQuat( orientation, out var forward, out var up );
			Render.DebugLine( position, position + forward             * 2.0f, Vector4.UnitY );
			Render.DebugLine( position, position + up                  * 2.0f, Vector4.UnitZ );
			Render.DebugLine( position, position + forward.Cross( up ) * 2.0f, Vector4.UnitX );

			if ( shapeTypeId != Mesh.Id )
			{
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
