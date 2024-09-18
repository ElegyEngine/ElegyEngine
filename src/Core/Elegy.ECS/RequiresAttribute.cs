// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public class RequiresAttribute<T> : Attribute
		where T : struct
	{
	}

	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public class RequiresAttribute<T1, T2> : Attribute
		where T1 : struct
		where T2 : struct
	{
	}

	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public class RequiresAttribute<T1, T2, T3> : Attribute
		where T1 : struct
		where T2 : struct
		where T3 : struct
	{
	}

	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public class RequiresAttribute<T1, T2, T3, T4> : Attribute
		where T1 : struct
		where T2 : struct
		where T3 : struct
		where T4 : struct
	{
	}

	[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
	public class RequiresAttribute<T1, T2, T3, T4, T5> : Attribute
		where T1 : struct
		where T2 : struct
		where T3 : struct
		where T4 : struct
		where T5 : struct
	{
	}
}
