using Elegy.ECS;

namespace Game.Shared.Components
{
	[Component]
	public partial struct Name
	{
		[Property]
		public string Targetname { get; set; }
	}
}
