// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Maths;
using Elegy.RenderSystem.API;
using Game.Shared.Components;
using Game.Shared.Physics;

namespace Game.Shared
{
	public class StandardPlayerController : IPlayerControllable
	{
		private Vector3 mPosition;
		private Vector3 mDirection;
		private Vector3 mViewAngles;

		public PhysicsShape Shape { get; private set; }

		public PhysicsBody Body { get; private set; }

		public void Setup( int entityId )
		{
			// TODO: nicer API for getting the entity..
			ref var transform = ref EntityWorld.GetEntityRef( entityId ).Ref<Transform>();

			mPosition = transform.Position;

			Shape = Physics.Physics.CreateShape( new BepuPhysics.Collidables.Cylinder( 0.5f, 2.0f ), 10.0f );
			Body = Physics.Physics.CreateKinematicBody( transform, Shape );
		}

		public void Update( float dt )
		{
			ref var motion = ref Body.BodyReference.Dynamics.Motion;

			mPosition = motion.Pose.Position;

			if ( !Body.BodyReference.Awake )
			{
				Physics.Physics.Simulation.Awakener.AwakenBody( Body.BodyHandle );
			}

			// Quick hacky little approximation until we get a proper character controller
			motion.Velocity.Linear += mDirection / (motion.Velocity.Linear.LengthSquared() + 0.25f);
		}

		public PlayerControllerState GenerateControllerState()
			=> new()
			{
				Position = mPosition,
				Angles = mViewAngles
			};

		public void HandleClientInput( ClientCommands commands )
		{
			mViewAngles = commands.ViewAngles;
			mDirection = commands.MovementDirection;
		}

		private readonly Vector3[] mBoxExtents =
		[
			new( 1.0f, 1.0f, 1.0f ), // 0
			new( -1.0f, 1.0f, 1.0f ), // 1
			new( -1.0f, 1.0f, -1.0f ), // 2
			new( 1.0f, 1.0f, -1.0f ), // 3
			new( 1.0f, -1.0f, 1.0f ), // 4
			new( -1.0f, -1.0f, 1.0f ), // 5
			new( -1.0f, -1.0f, -1.0f ), // 6
			new( 1.0f, -1.0f, -1.0f ), // 7
		];

		public void OnDebugDraw()
		{
			ref var motion = ref Body.BodyReference.Dynamics.Motion;

			Vector3 centre = motion.Pose.Position;
			Vector3 extent = new( 0.5f, 0.5f, 1.0f );

			void DrawLine( int a, int b )
			{
				Vector4 colour = new( 1.0f, 1.0f, 0.0f, 1.0f );
				Render.DebugLine( centre + mBoxExtents[a] * extent, centre + mBoxExtents[b] * extent, colour );
			}

			void DrawQuad( int a, int b, int c, int d )
			{
				DrawLine( a, b );
				DrawLine( b, c );
				DrawLine( c, d );
				DrawLine( d, a );
			}

			// Draw top square
			DrawQuad( 0, 1, 5, 4 );
			// Draw bottom square
			DrawQuad( 2, 3, 7, 6 );
			// Draw vertical bars
			DrawLine( 0, 3 );
			DrawLine( 1, 2 );
			DrawLine( 4, 7 );
			DrawLine( 5, 6 );
		}
	}
}
