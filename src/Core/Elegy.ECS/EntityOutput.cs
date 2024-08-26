// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using fennecs;

namespace Elegy.ECS
{
	public record struct EntityOutputEntry( string TargetEntity, string TargetInput, float FireDelay, string Parameters );

	public struct EntityOutput
	{
		public EntityOutput( int entityId, string name, List<EntityOutputEntry> entries )
		{
			Name = name;
			Entity = entityId;
			Entries = entries;
		}

		public string Name { get; }
		public int Entity { get; }
		public List<EntityOutputEntry> Entries { get; set; } = new();
	}
}
