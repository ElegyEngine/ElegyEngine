// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	public abstract class BaseRequiresAttribute : Attribute
	{
		public abstract Type ComponentType { get; }
	}

	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public class RequiresAttribute<T> : BaseRequiresAttribute
		where T : struct
	{
		public override Type ComponentType => typeof( T );
	}
}
