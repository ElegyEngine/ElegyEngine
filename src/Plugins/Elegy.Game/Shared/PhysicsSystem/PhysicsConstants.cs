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
	/// will still be reported.
	/// </summary>
	public enum CollisionLayer
	{
		/// <summary> Generic collision layer. </summary>
		General,

		/// <summary> Collision layer for triggers. </summary>
		Trigger,

		/// <summary> Liquids collision layer. </summary>
		Liquid,

		/// <summary> Purely adjusts the CoM, does not interact with other layers. </summary>
		WeightAdjustment
	}

	public static class CollisionLayerExtensions
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static CollisionResponse CanCollide( this CollisionLayer a, CollisionLayer b )
			=> (b > a ? (a, b) : (b, a)) switch
			{
				(CollisionLayer.General, CollisionLayer.General) => CollisionResponse.Block,
				(CollisionLayer.General, CollisionLayer.Trigger) => CollisionResponse.ReportOnly,
				(CollisionLayer.General, CollisionLayer.Liquid) => CollisionResponse.ReportOnly,
				_ => CollisionResponse.Discard
			};
	}

	/// <summary>
	/// Collision flags. The underlying algorithm is quite simple:
	/// if cm1 & cm2, it's a collision.
	/// </summary>
	[Flags]
	public enum ClipMask
	{
		/// <summary> No collision with anything. </summary>
		None = 0,

		/// <summary> Generic collision bit. </summary>
		General = 1,

		/// <summary> Stops +use rays. </summary>
		BlockUse = 2,

		/// <summary> Stops bullet/combat rays. </summary>
		BlockBullets = 4,

		/// <summary> Stops the player. Useful for fool-proofing the collision in MP maps. </summary>
		BlockPlayer = 8,

		/// <summary> Blocks visibility for NPCs. </summary>
		BlockSight = 16,

		/// <summary> Typical, opaque surface. Blocks everything. </summary>
		Solid = General | BlockUse | BlockBullets | BlockPlayer | BlockSight,

		/// <summary> Typical transparent surface. Permits sight rays. </summary>
		Transparent = Solid & ~BlockSight,

		/// <summary> Transparent surface with small holes. Lets bullets through. </summary>
		TransparentSmallHoles = Transparent & ~BlockBullets,

		/// <summary> Transparent surface with large holes. You can put your hand through it and use stuff. </summary>
		TransparentLargeHoles = TransparentSmallHoles & ~BlockUse
	}
}
