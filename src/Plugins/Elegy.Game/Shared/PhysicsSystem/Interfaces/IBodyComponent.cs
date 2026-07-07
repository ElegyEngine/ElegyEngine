namespace Game.Shared.PhysicsSystem.Interfaces
{
	public interface IBodyComponent
	{
		public PhysicsShape Shape { get; }
		public PhysicsBody BodyObject { get; }
	}

	public static class BodyComponentExtensions
	{
		public static void SetOwner( this IBodyComponent component, Entity entity )
		{
			if ( component.BodyObject.IsStatic )
			{
				Physics.Links[component.BodyObject.StaticHandle] = new( entity );
			}
			else
			{
				Physics.Links[component.BodyObject.BodyHandle] = new( entity );
			}
		}

		public static void SetLayer( this IBodyComponent component, CollisionLayer layer )
		{
			if ( component.BodyObject.IsStatic )
			{
				Physics.Filters.SetLayer( component.BodyObject.StaticHandle, layer );
			}
			else
			{
				Physics.Filters.SetLayer( component.BodyObject.BodyHandle, layer );
			}
		}
	}
}
