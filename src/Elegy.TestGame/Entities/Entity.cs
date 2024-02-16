// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Assets;

namespace TestGame.Entities
{
	public abstract class Entity
	{
		public Vector3 Position { get; set; }

		public virtual void Spawn()
		{

		}

		public virtual void PostSpawn()
		{

		}

		public virtual void KeyValue( Dictionary<string, string> pairs )
		{
			if ( pairs.TryGetValue( "origin", out string? originString ) )
			{

			}
		}

		public virtual void Destroy()
		{

		}

		public virtual void Think()
		{

		}

		public virtual void PhysicsUpdate( float delta )
		{

		}

		// This is a very very improper way to do this, but I needed it for a quick way of setting a brush model
		public void AddBrushModel( ElegyMapDocument map, int entityId )
		{

		}
	}
}
