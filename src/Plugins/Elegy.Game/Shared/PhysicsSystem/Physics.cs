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
using Game.Shared.PhysicsSystem.Interfaces;
using Game.Shared.PhysicsSystem.Subsystems;

namespace Game.Shared.PhysicsSystem
{
	public static partial class Physics
	{
		private static BufferPool mBufferPool;
		private static ThreadDispatcher mThreadDispatcher;

		public static Simulation Simulation { get; private set; }
		public static BroadPhase BroadPhase => Simulation.BroadPhase;
		public static ITimestepper Timestepper => Simulation.Timestepper;

		#region Physics systems

		public static ModularPhysicsCallbacks<IntegrationConfig> Callbacks = new();
		public static CharacterMovement Characters { get; } = new( characterCapacity: 1024 );
		public static DefaultGravity Gravity { get; } = new() { Gravity = Coords.Down * 9.81f };

		#endregion

		#region Physics details

		public struct IntegrationConfig : IIntegrationConfig
		{
			public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
			public bool AllowSubstepsForUnconstrainedBodies => false;
			public bool IntegrateVelocityForKinematics => false;
		}

		public struct NarrowphaseCallbacks : INarrowPhaseCallbacks
		{
			public void Initialize( Simulation simulation )
				=> Callbacks.Initialize( simulation );

			public bool AllowContactGeneration( int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin )
				=> Callbacks.AllowContactGeneration( workerIndex, a, b, ref speculativeMargin );

			public bool ConfigureContactManifold<TManifold>( int workerIndex, CollidablePair pair, ref TManifold manifold,
				out PairMaterialProperties pairMaterial ) where TManifold : unmanaged, IContactManifold<TManifold>
				=> Callbacks.ConfigureContactManifold( workerIndex, pair, ref manifold, out pairMaterial );

			public bool AllowContactGeneration( int workerIndex, CollidablePair pair, int childIndexA, int childIndexB )
				=> Callbacks.AllowContactGeneration( workerIndex, pair, childIndexA, childIndexB );

			public bool ConfigureContactManifold( int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold )
				=> Callbacks.ConfigureContactManifold( workerIndex, pair, childIndexA, childIndexB, ref manifold );

			public void Dispose()
				=> Callbacks.Dispose();
		}

		public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
		{
			public AngularIntegrationMode AngularIntegrationMode => Callbacks.AngularIntegrationMode;
			public bool AllowSubstepsForUnconstrainedBodies => Callbacks.AllowSubstepsForUnconstrainedBodies;
			public bool IntegrateVelocityForKinematics => Callbacks.IntegrateVelocityForKinematics;

			public void Initialize( Simulation simulation )
			{
				// Nothing, already done by NarrowphaseCallbacks
			}

			public void PrepareForIntegration( float dt )
				=> Callbacks.PrepareForIntegration( dt );

			public void IntegrateVelocity( Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia,
				Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity )
				=> Callbacks.IntegrateVelocity( bodyIndices, position, orientation, localInertia, integrationMask, workerIndex, dt, ref velocity );
		}

		#endregion

		public static void Init()
		{
			int numThreads = Environment.ProcessorCount - 2;

			// You can register your physics subsystems here, or anywhere really
			Callbacks.Register( Characters );
			Callbacks.Register( Gravity );

			mBufferPool = new( minimumBlockAllocationSize: 131072, expectedPooledResourceCount: 64 );
			mThreadDispatcher = new( numThreads );
			// Essentially, these narrowphase & pose integrator callbacks all call back into
			// our ModularPhysicsCallbacks which then handle everything from there, in a sorta unified way
			Simulation = Simulation.Create( mBufferPool,
				new NarrowphaseCallbacks(),
				new PoseIntegratorCallbacks(),
				// TODO: expose solver params
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
