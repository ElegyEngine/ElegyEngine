// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using TestGame.Client;

// Todo: investigate velocity clipping and friction
// investigate doing sliding manually (use MoveAndCollide instead of MoveAndSlide,
// should give much greater control)
// Also crouching

namespace TestGame.Entities
{
	public partial class Player : Entity
	{
		public const float Speed = 60.0f;
		public const float WalkSpeedThreshold = 60.0f;
		public const float AirSpeed = Speed * 0.95f;
		public const float AirSpeedThreshold = 2.8f;
		// 16 units converted to Godot units
		public const float Acceleration = 80.0f;
		public const float AirAcceleration = 100.0f;
		public const float AccelerationMultiplier = 15.0f;
		public const float Friction = 8.0f;
		public const float JumpForce = 400.0f;

		private void Move( float delta )
		{
			float speedModifier = 1.0f;
			if ( mLastCommands.ActionStates.HasFlag( ClientActions.Sprint ) )
			{
				speedModifier = 1.666f;
			}

			// On ground, apply friction'n'stuff
			//KinematicCollision3D collision = new();
			bool onGround = false; //mBody.TestMove( mBody.GlobalTransform, -Vector3.UnitZ * 0.02f, collision );
			
			if ( onGround )
			{
				WalkMove( delta, speedModifier, Vector3.UnitZ /*collision.GetNormal()*/ );
			}
			else
			{
				AirMove( delta, speedModifier );
			}

			//mBody.MoveAndSlide();
		}

		private void WalkMove( float delta, float speedModifier, Vector3 groundNormal )
		{
			float groundDot = groundNormal.Dot( Vector3.UnitZ );
			bool tooSteep = groundDot < 0.7f;
			if ( tooSteep )
			{
				speedModifier *= groundDot;
			}

			Accelerate( delta, mLastCommands.MovementDirection, Speed * speedModifier, Acceleration, 1.0f, WalkSpeedThreshold * speedModifier, Friction, groundNormal );
			
			// Cannot jump on slopes
			if ( !tooSteep )
			{
				// Friction
				//mBody.Velocity *= 0.85f;

				if ( mLastCommands.MovementDirection.Y > 0.0f )
				{
					//mBody.Velocity += Vector3.UnitZ * JumpForce * delta;
				}
			}
			else
			{
				AirFall( delta, groundNormal );
				//mBody.Velocity = ClipVelocity( delta, mBody.Velocity, groundNormal, 1.0f );
			}
		}

		private void AirMove( float delta, float speedMultiplier )
		{
			Accelerate( delta, mLastCommands.MovementDirection, AirSpeed * speedMultiplier, AirAcceleration, AccelerationMultiplier, AirSpeedThreshold, Friction, Vector3.UnitZ );
			AirFall( delta, Vector3.Zero );
		}

		private void Accelerate( float delta, Vector3 wishDirection, float speed, float acceleration, float accelerationMultiplier, float threshold, float friction, Vector3 groundNormal )
		{
			Vector3 moveDirectionFlat = wishDirection.XOZ();
			Vector3 moveDirectionFlatUnit = moveDirectionFlat.Normalized();
			Vector3 velocityFlat = Vector3.Zero; //mBody.Velocity.XOZ();

			float currentSpeed = velocityFlat.Dot( moveDirectionFlatUnit );
			float addSpeed = MathF.Min( threshold, speed ) - currentSpeed;

			if ( addSpeed <= 0.0f )
			{
				return;
			}

			float accelerationSpeed = MathF.Min( acceleration * speed * friction, addSpeed );
			//mBody.Velocity += accelerationSpeed * ClipVelocity( delta, moveDirectionFlat, groundNormal, 1.0f ) * delta * accelerationMultiplier;
		}

		const float GameBounce = 1.0f;
		const float GameFriction = 1.0f;

		private void AirFall( float delta, Vector3 groundNormal )
		{
			Vector3 downVelocity = -Vector3.UnitZ * 9.81f * 2.5f;
			
			if ( groundNormal != Vector3.Zero )
			{
				float dot = groundNormal.Dot( Vector3.UnitZ );
				downVelocity *= dot;
			}

			//mBody.Velocity += downVelocity * delta;
		}

		private Vector3 ClipVelocity( float delta, in Vector3 velocity, Vector3 groundNormal, float overbounce )
		{
			float backoff = velocity.Dot( groundNormal ) * overbounce;
			Vector3 change = groundNormal * backoff;
			return velocity - change;
		}
	}
}
