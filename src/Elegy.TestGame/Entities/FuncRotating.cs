// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Entities
{
	public class FuncRotating : Entity
	{
		public override void PostSpawn()
		{
			base.PostSpawn();
		}

		public override void PhysicsUpdate( float delta )
		{
			base.PhysicsUpdate( delta );
		
			mRootNode.RotateY( delta * 0.5f );
		}
	}
}
