// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.ECS;

public class World
{
	public World( int entityCapacity )
	{
		Entities = new( entityCapacity );
	}

	public Dictionary<Type, ISystem> SystemMap { get; } = new();

	public List<Entity> Entities;

	public int EntityCount => Entities.Count;

	public TEntity CreateEntity<TEntity>() where TEntity : Entity, new()
	{
		var result = new TEntity()
		{
			Id = Entities.Count,
			World = this
		};

		Entities.Add( result );
		return result;
	}

	public Entity? GetEntity( int id )
	{
		if ( id < 0 || id > Entities.Count )
		{
			return null;
		}

		return Entities[id];
	}

	public void RegisterSystem<TComponent>( ISystem system ) where TComponent : IComponent, new()
	{
		SystemMap[typeof( TComponent )] = system;
	}

	public void RegisterComponent<TComponent>( int capacity = 1000 ) where TComponent : IComponent, new()
	{
		SystemMap[typeof( TComponent )] = new System<TComponent>( this, capacity );
	}

	public System<TComponent> GetSystemFor<TComponent>() where TComponent : IComponent, new()
	{
		if ( SystemMap.TryGetValue( typeof( TComponent ), out var system ) )
		{
			return system as System<TComponent>;
		}

		throw new Exception();
	}

	public bool IsRegistered<TComponent>() where TComponent : IComponent, new()
	{
		return SystemMap.ContainsKey( typeof( TComponent ) );
	}
}
