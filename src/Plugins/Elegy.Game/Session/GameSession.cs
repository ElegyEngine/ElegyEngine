// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Client;
using Game.Shared;
using Game.Shared.Components;
using System.Diagnostics;

namespace Game.Session
{
	public partial class GameSession
	{
		public GameClient Client { get; }

		// TODO: For clientside prediction we should have
		// a couple copies of the entity world:
		// * the old snapshot
		// * the new snapshot
		// * currently predicted and interpolated state
		public EntityWorld EntityWorld { get; }

		public ref Entity ClientEntity => ref EntityWorld.Entities[ClientEntityId];
		public IPlayerControllable PlayerController => ClientEntity.Ref<Player>().Controller;

		public IServerBridge Bridge { get; set; }
		public AssetRegistry AssetRegistry => Bridge.AssetRegistry;

		public GameSession( GameClient client, EntityWorld world )
		{
			Client = client;
			EntityWorld = world;
		}

		public void Shutdown()
		{

		}

		public void Update( float delta )
		{
			if ( !FullyJoined )
			{
				return;
			}

			Debug.Assert( ClientEntityValid );

			Entity.ClientUpdateEvent data = new( ClientEntity, Client, delta );
			EntityWorld.Dispatch( data );

			PlayerController.HandleClientInput( Client.Commands );
			PlayerController.Update( delta );
		}
	}
}
