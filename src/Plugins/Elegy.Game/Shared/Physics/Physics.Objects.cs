// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Game.Shared.Components;

namespace Game.Shared.Physics
{
	public static partial class Physics
	{
		public static PhysicsBody CreateBody( in Transform worldTransform, PhysicsShape shape )
		{
			//bool needsConversion = shape.ShapeIndex.Type is Capsule.Id or Cylinder.Id;
			//Quaternion orientation = needsConversion ? PhysicsBody.ToBepu( worldTransform.Orientation ) : worldTransform.Orientation;
			Quaternion orientation = worldTransform.Orientation;

			PhysicsBody result = new(
				shape: shape,
				dynamicHandle: Simulation.Bodies.Add( BodyDescription.CreateDynamic(
					pose: new( worldTransform.Position, orientation ),
					inertia: shape.Inertia,
					collidable: shape.ShapeIndex,
					activity: 0.01f ) ),
				needsConversion: false );

			return result;
		}

		public static PhysicsBody CreateKinematicBody( in Transform worldTransform, PhysicsShape shape )
		{
			//bool needsConversion = shape.ShapeIndex.Type is Capsule.Id or Cylinder.Id;
			//Quaternion orientation = needsConversion ? PhysicsBody.ToBepu( worldTransform.Orientation ) : worldTransform.Orientation;
			Quaternion orientation = worldTransform.Orientation;

			PhysicsBody result = new(
				shape: shape,
				dynamicHandle: Simulation.Bodies.Add( BodyDescription.CreateDynamic(
					pose: new( worldTransform.Position, orientation ),
					// This type of inertia prevents unwanted rotations
					inertia: new() { InverseMass = shape.Inertia.InverseMass },
					collidable: new( shape.ShapeIndex, 0.1f, float.MaxValue, ContinuousDetection.Passive ),
					activity: 0.02f ) ),
				needsConversion: false );

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
					? meshShape.ComputeOpenInertia( MathF.Abs( mass ) )
					: meshShape.ComputeClosedInertia( MathF.Abs( mass ) )
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
	}
}
