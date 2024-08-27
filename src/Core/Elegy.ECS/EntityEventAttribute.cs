// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	public abstract class BaseEventAttribute : Attribute
	{
		public abstract Type EventHandlerType { get; }
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class EntityEventAttribute<T> : BaseEventAttribute
		where T: Delegate
	{
		public override Type EventHandlerType => typeof( T );
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class SystemEventAttribute<T> : BaseEventAttribute
		where T : Delegate
	{
		public override Type EventHandlerType => typeof( T );
	}
}
