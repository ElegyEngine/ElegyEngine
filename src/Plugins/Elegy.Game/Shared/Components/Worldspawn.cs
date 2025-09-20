// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Elegy.Common.Assets;
using Elegy.Common.Assets.ElegyMapData;
using Elegy.Common.Maths;
using Elegy.Common.Utilities;
using Elegy.LogSystem;
using Elegy.ECS;
using Elegy.RenderSystem.Objects;
using Game.Presentation;
using Game.Server;
using Mesh = Elegy.Common.Assets.MeshData.Mesh;

namespace Game.Shared.Components
{
	[Component]
	[Requires<StaticModel>]
	public partial struct Worldspawn
	{
		private static TaggedLogger mLogger = new( "Worldspawn" );
		[Property] public string Name { get; set; }

		[Event]
		public void OnMapLoad( Entity.OnMapLoadEvent data )
		{
			mLogger.Log( "OnMapLoad" );
		}

		[Event]
		public void OnClientSpawn( Entity.ClientSpawnEvent data )
		{
			mLogger.Log( "OnClientSpawn" );
		}
	}
}
