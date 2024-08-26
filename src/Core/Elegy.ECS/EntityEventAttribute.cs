// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	public abstract class BaseEntityEventAttribute : Attribute
	{
		public abstract Type EventHandlerType { get; }
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class EntityEventAttribute<T> : BaseEntityEventAttribute
		where T: Delegate
	{
		public override Type EventHandlerType => typeof( T );
	}
}
