// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Collections.Frozen;

namespace Elegy.ECS
{
	/// <summary>
	/// Data for event handling, as a complement to <see cref="fennecs.Archetype"/>.
	/// </summary>
	public class Archetype
	{
		/// <summary>
		/// Whether or not this archetype has been finished,
		/// and is ready to be used to handle events.
		/// </summary>
		public bool Ready { get; set; } = false;

		/// <summary>
		/// Component IDs encoded as a bitset. Component ID 1 would be the 1st bit and so on.
		/// </summary>
		public long[] ComponentMask { get; init; } = [];

		/// <summary>
		/// Parametreless event handlers. Refer to <see cref="EventHandlerAttribute"/> and <see cref="EventAttribute{T}"/>.
		/// </summary>
		public Dictionary<Type, List<Action<fennecs.Entity>>> EventHandlers { get; init; } = [];

		/// <summary>
		/// Event handlers with parametres. The parametre type is marked with <see cref="EventModelAttribute"/>.
		/// </summary>
		public Dictionary<Type, List<Action<fennecs.Entity, object>>> ComplexEventHandlers { get; init; } = [];

		public bool Matches( long[] componentMask )
			=> ComponentMask.Equals( componentMask );

		// TODO: move this into Elegy.Common into some bitmasking utils
		public static long[] EncodeComponentMask( IEnumerable<int> componentIds )
		{
			// Step 1: determine how many bits we're going to need
			int highest = componentIds.Max();
			// A long is 64 bits
			int numLongs = (highest / 64) + 1;

			// The rest is just populating the bits with the right positions
			long[] result = new long[numLongs];
			foreach ( var id in componentIds )
			{
				int slot = id / 64;
				int bit = id - (slot * 64);
				int position = 1 << bit;

				result[slot] |= (uint)position;
			}

			return result;
		}

		public static bool TestPosition( long[] componentIds, int id )
		{
			if ( id < 0 || id >= componentIds.Length * 64 )
			{
				return false;
			}

			int slot = id / 64;
			int bit = id - (slot * 64);
			int position = 1 << bit;

			return (componentIds[slot] & (uint)position) != 0;
		}

		public static List<int> DecodeComponentMask( long[] componentIds )
		{
			int highest = componentIds.Length * 64;

			List<int> result = [];
			for ( int i = 0; i < highest; i++ )
			{
				if ( TestPosition( componentIds, i ) )
				{
					result.Add( i );
				}
			}

			return result;
		}
	}
}
