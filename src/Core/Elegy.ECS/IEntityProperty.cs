// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	public interface IEntityProperty
	{
		/// <summary>
		/// Modifies itself with the parsed data.
		/// </summary>
		void ParseEntityProperty( fennecs.Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value );
	}
}
