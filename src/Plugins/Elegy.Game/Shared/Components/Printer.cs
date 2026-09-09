// SPDX-FileCopyrightText: 2026 Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Printer
	{
		[Property]
		public string Message { get; set; }

		[Input]
		public void Print()
		{
			Console.Submit( Message );
		}
	}
}
