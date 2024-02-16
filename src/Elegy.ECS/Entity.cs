// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.ECS;

public abstract class Entity
{
	public int Id { get; init; }

	public World World { get; init; }

	public ref TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
	{
		System<TComponent> system = World.GetSystemFor<TComponent>();

		return ref system.GetComponent( system.CreateComponent( this ) );
	}

	public ref TComponent GetComponent<TComponent>() where TComponent : IComponent, new()
	{
		System<TComponent> system = World.GetSystemFor<TComponent>();

		return ref system.GetComponentByEntity( Id );
	}

	public void RemoveComponent<TComponent>() where TComponent : IComponent, new()
	{
		System<TComponent> system = World.GetSystemFor<TComponent>();

		system.RemoveComponentForEntity( this );
	}

	public bool HasComponent<TComponent>() where TComponent : IComponent, new()
	{
		System<TComponent> system = World.GetSystemFor<TComponent>();
		
		return system.HasComponentForEntity( this );
	}

	public Entity? GetEntityOf<TComponent>( IComponent component ) where TComponent : IComponent, new()
	{
		System<TComponent> system = World.GetSystemFor<TComponent>();

		return system.GetEntity( component.Id );
	}
}
