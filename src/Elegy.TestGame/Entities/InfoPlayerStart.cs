// SPDX-FileCopyrightText: 2022-2023 Admer Šuko
// SPDX-License-Identifier: MIT

using Elegy.Assets;
using System.Diagnostics.CodeAnalysis;

namespace TestGame.Entities
{
	public class InfoPlayerStart : Entity
	{
		public override void Spawn()
		{
			base.Spawn();
			mRootNode = Nodes.CreateNode<Node3D>();
		}

		public Player SpawnPlayer( Game game )
		{
			var player = game.CreateEntity<Player>();
			player.Position += Position;
			return player;
		}
	}
}
