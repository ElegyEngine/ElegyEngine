// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Assets;
using Game.Shared;

namespace Game.Server
{
	public class EntityWorld
	{
		public fennecs.World EcsWorld { get; } = new( 4096 )
		{
			Name = "ServerEntityEcsWorld",
			GCBehaviour = fennecs.World.GCAction.ManualOnly
						| fennecs.World.GCAction.CompactStagnantArchetypes
						| fennecs.World.GCAction.DisposeEmptyArchetypes
						| fennecs.World.GCAction.DisposeEmptyRelationArchetypes
		};

		public List<Entity> Entities { get; } = new( 4096 );

		public bool Init()
		{
			return true;
		}

		public void Shutdown()
		{

		}

		public void Update( float delta )
		{

		}

		public bool Setup( ElegyMapDocument level )
		{
			return true;
		}

		public Entity CreateEntity()
		{
			Entity result = new( this, Entities.Count );
			Entities.Add( result );
			return result;
		}
	}
}
