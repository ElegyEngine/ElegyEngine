// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics;
using Elegy.Common.Maths;
using Elegy.RenderSystem.API;
using Game.Shared.Components;
using Game.Shared.PhysicsSystem;

namespace Game.Shared.Input
{
	public class StandardPlayerController : IClientController
	{
		public PhysicsShape Shape { get; private set; }
		public PhysicsBody Body { get; private set; }
		public BodyReference BodyRef => Body.BodyReference;
		public ref CharacterController Character => ref Physics.Characters.GetCharacter( BodyRef.Handle );

		public void Setup( int entityId )
		{
			// TODO: nicer API for getting the entity..
			ref var transform = ref EntityWorld.GetEntityRef( entityId ).Ref<Transform>();
			transform.Position += Coords.Up * 10.0f; // Spawn a little off the floor

			Shape = Physics.CreateShape( new BepuPhysics.Collidables.Cylinder( 0.35f, 1.8f ), 1.0f );
			Body = Physics.CreateKinematicBody( transform, Shape );

			ref var character = ref Physics.Characters.CreateCharacter( BodyRef.Handle );
			character.LocalUp = Coords.Up;
			character.CosMaximumSlope = 0.7f;
			character.JumpVelocity = 3.75f;
			character.MinimumSupportDepth = -0.002f;
			character.MinimumSupportContinuationDepth = -0.1f;
			character.MaximumVerticalForce = 100.0f;
			character.MaximumHorizontalForce = 35.0f;
			character.ViewDirection = Coords.Forward;
		}

		public PlayerControllerState Update( float dt, ClientCommand command )
		{
			ref var motion = ref Body.BodyReference.Dynamics.Motion;

			if ( !Body.BodyReference.Awake )
			{
				Physics.Simulation.Awakener.AwakenBody( Body.BodyHandle );
			}

			if ( command.MovementDirection.Z > 0.95f )
			{
				Character.TryJump = true;
			}

			Vector3 viewFlatForward = Coords.DirectionFromYawDegrees( command.ViewAngles.Y );
			Vector3 viewFlatRight = viewFlatForward.Cross( Coords.Up );
			Vector3 movement = command.MovementDirection.Y * viewFlatForward
			                   + command.MovementDirection.X * viewFlatRight;

			Character.TargetVelocity = movement.ToVector2() * 3.3f;

			return new()
			{
				Position = motion.Pose.Position,
				Angles = command.ViewAngles
			};
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
