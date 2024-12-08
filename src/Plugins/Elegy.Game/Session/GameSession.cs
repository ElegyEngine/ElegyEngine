﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Client;
using Game.Shared;
using Game.Shared.Components;
using System.Diagnostics;
using Elegy.Common.Maths;

namespace Game.Session
{
	public partial class GameSession
	{
		public GameClient Client { get; }
		
		public ref Entity ClientEntity => ref EntityWorld.Entities[ClientEntityId];
		public IPlayerControllable PlayerController => ClientEntity.Ref<Player>().Controller;

		public IServerBridge Bridge { get; set; }
		public AssetRegistry AssetRegistry => Bridge.AssetRegistry;

		public GameSession( GameClient client )
		{
			Client = client;

			Client.RenderView.Projection = Coords.CreatePerspectiveMatrix( MathF.PI / 2.0f, 16.0f / 9.0f, 0.01f, 4096.0f );
			Client.RenderView.Transform = Coords.CreateViewMatrix( Vector3.Zero, Coords.Forward, Coords.Up );
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

			// TODO: Move this someplace more appropriate
			var state = PlayerController.GenerateControllerState();
			Client.RenderView.Transform = Coords.CreateViewMatrixDegrees( state.Position, state.Angles );
		}
	}
}
