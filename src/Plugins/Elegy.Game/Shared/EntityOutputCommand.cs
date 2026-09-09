// SPDX-FileCopyrightText: 2026 Elegy Engine contributors
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace Game.Shared
{
	[StructLayout( LayoutKind.Sequential )]
	public struct EntityOutputCommand
	{
		public Entity Sender; // 64
		public Entity Receiver; // 64
		public long ExecutionTime; // 64
		public long InputId; // 64, EntityUtilities.ComponentInput

		/* Alternative structure:
		 * Sender - i32 id (4 bytes)
		 * Receiver - i32 id (4 bytes)
		 *            ^ storing the whole ECS entity is faster but eh
		 * Exectime + input:
		 * - Exectime - u48 usec (6 bytes)
		 * - Input - u16 id (2 bytes)
		 * This reduces it to a total of 16 bytes. We can then add another 16 bytes of
		 * data at the end, which could be used for parametres and such.
		 */
	}
}
