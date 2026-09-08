using System.Runtime.InteropServices;

namespace Game.Shared
{
	[StructLayout( LayoutKind.Sequential )]
	public struct EntityOutputCommand
	{
		public fennecs.Entity Sender; // 64
		public fennecs.Entity Receiver; // 64
		public long ExecutionTime; // 64
		public long InputId; // 64, EntityUtilities.ComponentInput
	}
}
