// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Shared;

namespace Game.Session
{
	public partial class GameSession
	{
		public bool FullyJoined { get; private set; } = false;
		public int ClientEntityId { get; private set; } = -1;
		public bool ClientEntityValid => ClientEntityId > 0 && ClientEntity.Alive;

		public void SendClientState()
		{
			Bridge.SendInputPayload( Client.Commands );
		}

		public void ReceiveSpawnResponse( int entityId )
		{
			ClientEntityId = entityId;
			PlayerController.Setup( EntityWorld );
			ClientEntity.Dispatch<Entity.ClientPossessedEvent>( new( ClientEntity ) );

			Bridge.SendSpawnComplete();
		}

		public void ReceiveDisconnect( string reason )
		{
			//Client.DisplayDisconnectMenu( reason );
			Shutdown();
		}

		public void ReceiveSpawnCompleteAck()
		{
			FullyJoined = true;
		}
	}
}
