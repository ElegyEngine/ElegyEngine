﻿// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;

namespace Game.Shared
{
	public record struct EntityOutputEntry( string TargetEntity, string TargetInput, float FireDelay, string Parameter )
	{
		/// <summary>
		/// Input formats:
		/// <code>
		/// "targetname,component.input,delay"
		/// "targetname,component.input,delay,parametre"
		/// </code>
		/// </summary>
		public static EntityOutputEntry ParseEntry( ReadOnlySpan<char> value )
		{
			int firstComma = value.IndexOf( ',' );
			int secondComma = value.Slice( firstComma + 1 ).IndexOf( ',' ) + firstComma + 1;
			int thirdComma = value.LastIndexOf( ',' );
			bool parametreless = secondComma == thirdComma;

			var target = value.Slice( 0, firstComma ).Trim();
			var input = value.Slice( firstComma + 1, secondComma - firstComma - 1 ).Trim();
			var delay = (parametreless ? value.Slice( secondComma + 1 ) : value.Slice( secondComma + 1, thirdComma - secondComma - 1 )).Trim();
			var parametre = parametreless ? "" : value.Slice( thirdComma + 1 ).Trim();

			return new( target.ToString(), input.ToString(), Parse.Float( delay ), parametre.ToString() );
		}

		/// <summary>
		/// Input format:
		/// <code>
		/// "targetname1,component.input1,delay1,parametre1;targetname2,component.input2,delay2,parametre2"
		/// </code>
		/// </summary>
		public static List<EntityOutputEntry> ParseOutput( ReadOnlySpan<char> value )
		{
			// This seems to be a good way to estimate how many output entries we'll have
			List<EntityOutputEntry> result = new( value.Count( ';' ) + 1 );

			// This is not exactly a copy of the value,
			// but a copy of the *view* into the value
			ReadOnlySpan<char> valueCopy = value;

			// Splitting up the value string is done manually because
			// we don't have an adequate Split method.. could honestly
			// go the functional route and have an action of some sorts,
			// when C# gets ref struct support in delegates anyway
			while ( true )
			{
				var semicolon = valueCopy.IndexOf( ';' );
				var entry = semicolon < 0 ? valueCopy : valueCopy.Slice( 0, semicolon ).Trim();
				result.Add( ParseEntry( entry ) );

				if ( semicolon == -1 )
				{
					break;
				}

				valueCopy = valueCopy.Slice( semicolon + 1 );
			}

			// Outputs should be sorted by time of execution, shortest to longest
			result.Sort( ( e1, e2 ) =>
			{
				return (int)(e1.FireDelay - e2.FireDelay);
			} );

			return result;
		}
	}

	public struct EntityOutput : IEntityProperty
	{
		public EntityOutput() { }

		public EntityOutput( Entity entity, string name, List<EntityOutputEntry> entries )
		{
			Name = name;
			Entity = entity;
			Entries = entries;
		}

		public void ParseEntityProperty( fennecs.Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value )
		{
			this = new( entity.Ref<Entity>(), key.ToString(), EntityOutputEntry.ParseOutput( value ) );
		}

		public void Fire()
		{
			foreach ( var entry in Entries.AsSpan() )
			{
				//Entity.World.QueueNamedEvent( entry );
			}
		}

		public string Name { get; }
		public Entity Entity { get; }
		public List<EntityOutputEntry> Entries { get; set; } = new();
	}
}