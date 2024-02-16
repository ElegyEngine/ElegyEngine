// SPDX-FileCopyrightText: 2024 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Elegy.ECS;

public struct Bucket<TComponent> where TComponent : IComponent, new()
{
	public const int ComponentsPerBucket = 256;
	public const int FlagsPerBucket = ComponentsPerBucket / sizeof( long );

	public TComponent[] Components { get; } = new TComponent[ComponentsPerBucket];

	/// <summary>
	/// So that components can know which entity they belong to.
	/// </summary>
	public int[] ComponentEntityTable { get; } = new int[ComponentsPerBucket];

	public long[] LifeFlags { get; } = new long[FlagsPerBucket];

	public Bucket()
	{
		Array.Fill( ComponentEntityTable, -1 );
		Array.Fill( LifeFlags, 0 );
	}

	public int GetEntityIndex( int index )
	{
		if ( !ExistsComponent( index ) )
		{
			return -1;
		}

		return ComponentEntityTable[index];
	}

	public int CreateComponent( int entityIndex )
	{
		int componentIndex = FindFreeIndex();
		if ( componentIndex == -1 )
		{
			return -1;
		}

		ComponentEntityTable[componentIndex] = entityIndex;
		SetComponentState( componentIndex, true );
		Components[componentIndex].Id = componentIndex;

		return componentIndex;
	}

	public int FindFreeIndex()
	{
		for ( int i = 0; i < FlagsPerBucket; i++ )
		{
			long flags = LifeFlags[i];
			if ( flags == long.MaxValue )
			{
				continue;
			}

			if ( flags == 0 )
			{
				return i * ComponentsPerBucket;
			}
		}

		return -1;
	}

	public bool HasFreeIndices()
	{
		for ( int i = 0; i < FlagsPerBucket; i++ )
		{
			if ( LifeFlags[i] != long.MaxValue )
			{
				return true;
			}
		}

		return false;
	}

	public ref TComponent GetComponent( int index )
	{
		return ref Components[index];
	}

	public void SetComponentState( int index, bool value )
	{
		int flagIndex = index / FlagsPerBucket;
		int position = index % FlagsPerBucket;

		if ( value )
		{
			LifeFlags[flagIndex] |= 1U << position;
		}
		else
		{
			LifeFlags[flagIndex] &= ~(1U << position);
			ComponentEntityTable[index] = -1;
			Components[index].Id = -1;
		}
	}

	public bool ExistsComponent( int index )
	{
		int flagIndex = index / FlagsPerBucket;
		int position = index % FlagsPerBucket;

		if ( (LifeFlags[flagIndex] & (1U << position)) == 0 )
		{
			return false;
		}

		return true;
	}
}
