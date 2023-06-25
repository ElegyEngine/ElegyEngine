// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy
{
	/// <summary>
	/// Effectively the entry point into the engine. The Godot-based launcher app
	/// loads this DLL, calls EntryPoint.Init and so on.
	/// </summary>
	public static class EntryPoint
	{
		/// <summary>
		/// The entry point, effectively our Main method.
		/// </summary>
		public static bool Init( Node3D rootNode )
		{
			mEngine = new( rootNode, OS.GetCmdlineUserArgs() );
			return mEngine.Init();
		}

		/// <summary>
		/// Called every frame by Elegy.Launcher.
		/// </summary>
		public static void Update( float delta )
		{
			mEngine.Update( delta );
		}

		/// <summary>
		/// Called every physics frame by Elegy.Launcher.
		/// </summary>
		public static void PhysicsUpdate( float delta )
		{
			mEngine.PhysicsUpdate( delta );
		}

		/// <summary>
		/// Called for every input event by Elegy.Launcher.
		/// </summary>
		public static void HandleInput( InputEvent @event )
		{
			mEngine.HandleInput( @event );
		}

		private static Engine mEngine;
	}
}
