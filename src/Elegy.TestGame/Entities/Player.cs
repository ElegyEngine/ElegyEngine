// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using TestGame.Client;

namespace TestGame.Entities
{
	public partial class Player : Entity, IPlayerControllable
	{
		public const float PlayerHeight = 1.83f;
		public const float EyeHeight = 1.74f;

		public override void Spawn()
		{
			/*
			mBody = Nodes.CreateNode<CharacterBody3D>();
			mBody.GlobalPosition += Vector3.Up * 1.5f;

			mCapsule = new CapsuleShape3D();
			mCapsule.Radius = 0.5f;
			mCapsule.Height = PlayerHeight;

			mShape = mBody.CreateChild<CollisionShape3D>();
			mShape.Shape = mCapsule;

			mRootNode = mBody;
			*/
		}

		public override void PhysicsUpdate( float delta )
		{
			Move( delta );

			//mBody.GlobalRotation = new Vector3( 0.0f, mLastCommands.ViewAngles.Y, 0.0f );
		}

		public void HandleClientInput( ClientCommands commands )
		{
			mLastCommands = commands;
		}

		public PlayerControllerState GenerateControllerState()
		{
			return new()
			{
				Position = Position, //mBody.GlobalPosition + Vector3.Up * EyeHeight * 0.5f,
				Angles = Vector3.Zero //mBody.GlobalRotation
			};
		}

		ClientCommands mLastCommands;
		
		//CharacterBody3D mBody;
		//CollisionShape3D mShape;
		//CapsuleShape3D mCapsule;
	}
}
