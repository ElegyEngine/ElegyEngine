// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Game.Shared;

namespace Elegy.Game.Tests;

public static class EntityOutputTest
{
	internal static void AssertEntityOutputEntry( EntityOutputEntry a, EntityOutputEntry b )
	{
		Assert.Equal( a.TargetEntity, b.TargetEntity );
		Assert.Equal( a.TargetInput, b.TargetInput );
		Assert.Equal( a.FireDelay, b.FireDelay );
		Assert.Equal( a.Parameter, b.Parameter );
	}

	[Fact]
	public static void TestParsing()
	{
		EntityWorld.Init( 16 );

		var entity = EntityWorld.CreateEntity().FinishSpawning();

		EntityOutput entityOutput = EntityOutput.ParseEntityProperty( ref entity.EcsObjectRef, "Component.Output",
			"some_entity,Breakable.Break,3;" +
			// This one starts earlier, so they should be sorted in reverse order
			"some_entity,Breakable.Break,2,1"
		);

		Assert.Equal( 2, entityOutput.Entries.Count );

		AssertEntityOutputEntry( entityOutput.Entries[0], new( "some_entity", "Breakable.Break", 2.0f, "1" ) );
		AssertEntityOutputEntry( entityOutput.Entries[1], new( "some_entity", "Breakable.Break", 3.0f, "" ) );

		EntityWorld.Shutdown();
	}
}