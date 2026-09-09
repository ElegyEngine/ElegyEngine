// SPDX-FileCopyrightText: 2026 Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics;
using BepuPhysics.Collidables;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.PhysicsSystem.Subsystems
{
	public class CollisionFiltering<TMask> : IPhysicsSubsystem
		where TMask : unmanaged, Enum
	{
		private CollidableProperty<CollisionLayer> mLayers;

		public void Init( Simulation simulation )
		{
			mLayers = new( simulation );
		}

		public void SetLayer( BodyHandle handle, CollisionLayer layer )
			=> mLayers[handle] = layer;

		public void SetLayer( StaticHandle handle, CollisionLayer layer )
			=> mLayers[handle] = layer;

		public CollisionLayer GetLayer( BodyHandle handle )
			=> mLayers[handle];

		public CollisionLayer GetLayer( StaticHandle handle )
			=> mLayers[handle];

		public CollisionResponse CanCollide( CollidableReference a, CollidableReference b )
		{
			CollisionLayer layerA = mLayers[a];
			CollisionLayer layerB = mLayers[b];

			return layerA.CanCollide( layerB );
		}
	}
}
