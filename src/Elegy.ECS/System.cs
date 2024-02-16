// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.ECS;

public interface IComponent
{
	int Id { get; set; }
}

public interface ISystem
{
	World World { get; }

	Entity? GetEntity( int componentIndex );

	int CreateComponent( Entity entity );
}

public class System<TComponent> : ISystem
	where TComponent : IComponent, new()
{
	private Bucket<TComponent>[] mBuckets;

	public const int ComponentsPerBucket = Bucket<TComponent>.ComponentsPerBucket;

	public Bucket<TComponent>[] Buckets => mBuckets;

	public World World { get; }

	/// <summary>
	/// So that entities can know which component they have.
	/// </summary>
	public int[] EntityComponentTable { get; }

	public System( World world, int capacity = 1000 )
	{
		World = world;
		mBuckets = new Bucket<TComponent>[capacity / ComponentsPerBucket + 1];

		EntityComponentTable = new int[World.EntityCount];
		Array.Fill( EntityComponentTable, -1 );
	}

	public Entity? GetEntity( int componentIndex )
	{
		int bucketId = GetBucketId( componentIndex );
		if ( bucketId > Buckets.Length )
		{
			return null;
		}

		if ( !Buckets[bucketId].ExistsComponent( componentIndex % ComponentsPerBucket ) )
		{
			return null;
		}

		int entityIndex = Buckets[bucketId].GetEntityIndex( componentIndex % ComponentsPerBucket );
		if ( entityIndex == -1 )
		{
			return null;
		}

		return World.GetEntity( entityIndex );
	}

	public void RemoveEntity( Entity entity )
	{
		int componentToRemove = EntityComponentTable[entity.Id];
		if ( componentToRemove == -1 )
		{
			return;
		}

		RemoveComponent( componentToRemove );
	}

	public int CreateComponent( Entity entity )
	{
		for ( int i = 0; i < Buckets.Length; i++ )
		{
			int index = Buckets[i].FindFreeIndex();
			if ( index != -1 )
			{
				int componentIndex = i * ComponentsPerBucket + index;
				EntityComponentTable[componentIndex] = entity.Id;

				return componentIndex;
			}
		}

		// If all fails, create a new bucket
		Array.Resize( ref mBuckets, mBuckets.Length + 1 );

		int componentIndexNewBucket = (mBuckets.Length - 1) * ComponentsPerBucket;
		EntityComponentTable[componentIndexNewBucket] = entity.Id;

		return componentIndexNewBucket;
	}

	public ref TComponent GetComponent( int componentIndex )
	{
		return ref Buckets[GetBucketId( componentIndex )]
			.GetComponent( componentIndex % ComponentsPerBucket );
	}

	public ref TComponent GetComponentByEntity( int entityIndex )
	{
		int componentIndex = EntityComponentTable[entityIndex];
		return ref GetComponent( componentIndex );
	}

	public void RemoveComponent( int componentIndex )
	{
		Buckets[GetBucketId( componentIndex )]
			.SetComponentState( componentIndex % ComponentsPerBucket, false );

		EntityComponentTable[componentIndex] = -1;
	}

	public void RemoveComponentForEntity( Entity entity )
	{
		if ( EntityComponentTable[entity.Id] == -1 )
		{
			return;
		}

		RemoveComponent( EntityComponentTable[entity.Id] );
	}

	public bool HasComponent( int componentIndex )
	{
		return Buckets[GetBucketId( componentIndex )]
			.ExistsComponent( componentIndex % ComponentsPerBucket );
	}

	public bool HasComponentForEntity( Entity entity )
	{
		return HasComponent( EntityComponentTable[entity.Id] );
	}

	public void ForEach( Action<TComponent> action )
	{
		for ( int i = 0; i < mBuckets.Length; i++ )
		{
			for ( int c = 0; c < ComponentsPerBucket; c++ )
			{
				if ( mBuckets[i].ExistsComponent( c ) )
				{
					action( mBuckets[i].GetComponent( c ) );
				}
			}
		}
	}

	private int GetBucketId( int componentIndex )
	{
		return componentIndex / ComponentsPerBucket;
	}
}
