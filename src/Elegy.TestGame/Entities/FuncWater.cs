// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace TestGame.Entities
{
	public class FuncWater : Entity
	{
		public override void PostSpawn()
		{
			base.PostSpawn();

			StaticBody3D body = mRootNode.GetChild<StaticBody3D>( 1 );
			if ( body != null )
			{
				// Make non-solid
				body.CollisionLayer = 0;
			}
		}
	}
}
