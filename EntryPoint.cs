// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	public static class EntryPoint
	{
		public static bool Init( Node3D rootNode )
		{
			mEngine = new( rootNode, OS.GetCmdlineArgs() );
			return mEngine.Init();
		}

		public static void Update( float delta )
		{
			mEngine.Update( delta );
		}

		public static void PhysicsUpdate( float delta )
		{
			mEngine.PhysicsUpdate( delta );
		}

		public static void HandleInput( InputEvent @event )
		{
			mEngine.HandleInput( @event );
		}

		private static Engine mEngine;
	}
}
