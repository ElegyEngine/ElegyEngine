// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	[Requires<Transform>]
	public partial struct Player
	{
		[EventModel] public record struct PlayerSpawnedEvent( Player player );

		public Player() { }

		public bool IsLocal { get; set; }

		public IPlayerControllable Controller { get; set; } = new BasicController();

		[Event]
		public void Spawn( Entity.SpawnEvent data )
		{
			// Notify entities that a player has spawned
			data.Self.World.Dispatch<PlayerSpawnedEvent>( new( this ) );

			// Set up the controller so that it can
			// collide against the world and so on
			Controller.Setup( data.Self.World );
		}

		// Updates all players on the server
		[GroupEvent]
		public static void ServerUpdate( Entity.ServerUpdateEvent data, ref Player player )
		{
			if ( player.IsLocal )
			{
				return;
			}

			player.Controller.Update( data.Delta );
		}

		[Event]
		public void OnClientPossess( Entity.ClientPossessedEvent data )
		{
			IsLocal = true;
		}
	}
}
