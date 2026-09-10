// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;
using Game.Shared.PhysicsSystem;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.Components
{
	[Component]
	[Requires<StaticModel>]
	[Requires<BodyStatic>]
	public partial struct Worldspawn
	{
		private static TaggedLogger mLogger = new( "Worldspawn" );
		[Property] public string Name { get; set; }

		[Event]
		public void OnSpawn( SpawnEvent data )
		{
			data.Self.Ref<BodyStatic>().SetLayer( CollisionLayer.World );
		}

		[Event]
		public void OnMapLoad( OnMapLoadEvent data )
		{
			mLogger.Log( "OnMapLoad" );
		}

		[Event]
		public void OnClientSpawn( ClientSpawnEvent data )
		{
			mLogger.Log( "OnClientSpawn" );
		}
	}
}
