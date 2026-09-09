// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.Common.Utilities;
using Elegy.ECS;

namespace Game.Shared
{
	[Flags]
	public enum OutputFlags
	{
		None = 0,
		FireOnce = 1
	}

	public record struct EntityOutputEntry( string TargetEntity, string TargetInput, float FireDelay, string Parameter, OutputFlags Flags )
	{
		/// <summary>
		/// Input format:
		/// <code>
		/// "name,component.input,param,delay,flags"
		/// </code>
		/// </summary>
		public static EntityOutputEntry ParseEntry( ReadOnlySpan<char> value )
		{
			Span<Range> ranges = stackalloc Range[5];
			value.Split( ranges, ',', StringSplitOptions.TrimEntries );

			string target = value[ranges[0]].ToString();
			string input = value[ranges[1]].ToString();
			string param = value[ranges[2]].ToString();
			string delay = value[ranges[3]].ToString();
			string flags = value[ranges[4]].ToString();

			// Flags are 1 by default
			if ( string.IsNullOrEmpty( flags ) )
			{
				flags = "1";
			}

			return new( target, input, Parse.Float( delay ), param, (OutputFlags)Parse.Int( flags ) );
		}

		/// <summary>
		/// Input format:
		/// <code>
		/// "name1,component.input1,param1,delay1,flags1;name2,component.input2,param2,delay2,flags2"
		/// </code>
		/// </summary>
		public static List<EntityOutputEntry> ParseOutput( ReadOnlySpan<char> value )
		{
			// This seems to be a good way to estimate how many output entries we'll have
			List<EntityOutputEntry> result = new( value.Count( ';' ) + 1 );

			// This is not exactly a copy of the value,
			// but a copy of the *view* into the value
			// TODO: ??? This seems unnecessary
			ReadOnlySpan<char> valueCopy = value;

			// TODO: Use .Split or something similar
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
			result.Sort( static ( e1, e2 ) => (int)(e1.FireDelay - e2.FireDelay) );

			return result;
		}
	}

	public struct EntityOutput : IEntityProperty<EntityOutput>
	{
		public EntityOutput( ref Entity entity, string name, List<EntityOutputEntry> entries )
		{
			Name = name;
			Entity = entity;
			Entries = entries;
		}

		public static EntityOutput ParseEntityProperty( ref Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value )
			=> new( ref entity, key.ToString(), EntityOutputEntry.ParseOutput( value ) );

		public void Fire()
		{
			foreach ( var entry in Entries.AsSpan() )
			{
				if ( HasFired && entry.Flags.HasFlag( OutputFlags.FireOnce ) )
				{
					continue;
				}

				EntityWorld.QueueOutput( Entity, entry );
			}

			HasFired = true;
		}

		public string Name { get; }
		public Entity Entity { get; }
		public List<EntityOutputEntry> Entries { get; set; } = new();
		public bool HasFired { get; private set; }
	}
}
