// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.ECS;

public struct BreakableComponent : IComponent
{
	public int Id { get; set; }

	public BreakableComponent()
	{

	}
}

public struct GenericComponent : IComponent
{
	public int Id { get; set; }

	public GenericComponent()
	{

	}
}

public class BreakableSystem : System<BreakableComponent>
{
	public BreakableSystem( World world, int capacity )
		: base( world, capacity )
	{

	}
}

public class Door : Entity
{
	public void Spawn()
	{
		AddComponent<BreakableComponent>();
	}

	public void OnOpen( Entity opener )
	{
		if ( opener.HasComponent<BreakableComponent>() )
		{
			// do stuff
		}
	}
}

public static class EcsTest
{
	public static void Test()
	{
		World world = new( 1024 );
		world.RegisterComponent<GenericComponent>( 1000 );
		world.RegisterSystem<BreakableComponent>( new BreakableSystem( world, 1000 ) );

		Door door = world.CreateEntity<Door>();
		door.Spawn();
		door.AddComponent<GenericComponent>();
		door.AddComponent<BreakableComponent>();

	}
}
