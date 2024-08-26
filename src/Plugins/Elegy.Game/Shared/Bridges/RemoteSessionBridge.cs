﻿// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Server;
using Game.Session;
using System.Net;

namespace Game.Shared.Bridges
{
	public class RemoteSessionBridge : ISessionBridge
	{
		public RemoteSessionBridge( IPAddress hostAddress )
		{

		}

		public AssetRegistry GetAssetRegistry()
		{
			throw new NotImplementedException();
		}

		public List<Entity> GetEntities()
		{
			throw new NotImplementedException();
		}

		public void SendInputSnapshot( in ClientCommands snapshot )
		{
			throw new NotImplementedException();
		}

		public void Update( float delta )
		{
			throw new NotImplementedException();
		}
	}
}
