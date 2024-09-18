// SPDX-FileCopyrightText: 2024-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

namespace Elegy.ECS
{
	[AttributeUsage( AttributeTargets.Method )]
	public class EventAttribute : Attribute { }

	[AttributeUsage( AttributeTargets.Method )]
	public class GroupEventAttribute : Attribute { }
}
