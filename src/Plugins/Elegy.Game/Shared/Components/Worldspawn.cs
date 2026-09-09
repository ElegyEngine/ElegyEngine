// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;

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
