// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	public interface IEntityProperty<out T>
	{
		/// <summary>
		/// Modifies itself with the parsed data.
		/// </summary>
		static abstract T ParseEntityProperty( ref fennecs.Entity entity, ReadOnlySpan<char> key, ReadOnlySpan<char> value );
	}
}
