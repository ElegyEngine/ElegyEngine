using System.Runtime.CompilerServices;

namespace Game.Shared.PhysicsSystem
{
	/// <summary>
	/// Collision layer response.
	/// </summary>
	public enum CollisionResponse
	{
		/// <summary> Generates a contact and blocks the two bodies from passing through each other. </summary>
		Block,

		/// <summary> Generates a contact but lets the bodies pass through each other. </summary>
		ReportOnly,

		/// <summary> Does not generate a contact, bodies pass through each other. </summary>
		Discard
	}

	/// <summary>
	/// Collision layers. They define which body may pass through which.
	/// For example, a <see cref="General"/> body passing through a
	/// <see cref="Trigger"/> will not be blocked by it, but a collision
	/// will still be reported for game logic purposes.
	/// </summary>
	public enum CollisionLayer
	{
		/// <summary> Generic collision layer: NPCs, items and the like. </summary>
		General,

		/// <summary> Static world collision. </summary>
		World,

		/// <summary> Triggers. They interact with game entities but not the world. </summary>
		Trigger,

		/// <summary> Same as Trigger. </summary>
		Liquid,

		/// <summary> Centre-of-mass adjustment for vehicles, does not interact with other layers. </summary>
		WeightAdjustment
	}

	public static class CollisionLayerExtensions
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static CollisionResponse CanCollide( this CollisionLayer a, CollisionLayer b )
			=> (b > a ? (a, b) : (b, a)) switch
			{
				(CollisionLayer.General, CollisionLayer.World) => CollisionResponse.Block,
				(CollisionLayer.General, CollisionLayer.General) => CollisionResponse.Block,
				(CollisionLayer.General, CollisionLayer.Trigger) => CollisionResponse.ReportOnly,
				(CollisionLayer.General, CollisionLayer.Liquid) => CollisionResponse.ReportOnly,
				_ => CollisionResponse.Discard
			};
	}

	/// <summary>
	/// Collision flags. The underlying algorithm is quite simple:
	/// if cm1 & cm2, it's a collision.
	///
	/// Materials would declare their own clip masks for collision purposes.
	/// </summary>
	[Flags]
	public enum ClipMask
	{
		/// <summary> No collision with anything. </summary>
		None = 0,

		/// <summary> Generic collision bit. </summary>
		General = 1,

		/// <summary> Stops +use rays. </summary>
		Use = 2,

		/// <summary> Stops bullet/combat rays. </summary>
		Bullets = 4,

		/// <summary> Stops the player. Useful for fool-proofing the collision in MP maps. </summary>
		Player = 8,

		/// <summary> Blocks visibility for NPCs. </summary>
		Sight = 16,

		/// <summary> Typical, opaque surface. Blocks everything. </summary>
		Solid = General | Use | Bullets | Player | Sight,

		/// <summary> Typical transparent surface. Permits sight rays. </summary>
		Transparent = Solid & ~Sight,

		/// <summary> Transparent surface with small holes. Lets bullets through. </summary>
		TransparentSmallHoles = Transparent & ~Bullets,

		/// <summary> Transparent surface with large holes. You can put your hand through it and use stuff. </summary>
		TransparentLargeHoles = TransparentSmallHoles & ~Use
	}
}
