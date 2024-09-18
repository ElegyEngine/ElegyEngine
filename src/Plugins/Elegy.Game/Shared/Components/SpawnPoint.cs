// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	[Requires<Transform>]
	public partial struct SpawnPoint
	{
		public SpawnPoint() { }

		public static List<int> SpawnPointIds { get; } = new( 32 );

		[Property]
		public bool Active { get; set; } = true;

		[Property]
		public EntityOutput OnPlayerSpawn { get; set; } = new();

		[Event]
		public void Spawn( Entity.SpawnEvent data )
		{
			// Bookkeep entity IDs for the spawn system
			SpawnPointIds.Add( data.Self.Id );
		}

		[Input]
		public void Enable()
		{
			Active = true;
		}

		[Input]
		public void Disable()
		{
			Active = false;
		}

		[Input]
		public void Toggle()
		{
			Active = !Active;
		}

		[Event]
		public void PlayerSpawned( Player.PlayerSpawnedEvent data )
		{
			OnPlayerSpawn.Fire();
		}
	}
}
