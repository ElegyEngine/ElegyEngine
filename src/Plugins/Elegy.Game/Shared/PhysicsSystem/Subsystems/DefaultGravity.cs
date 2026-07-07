using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuUtilities;
using Game.Shared.PhysicsSystem.Interfaces;

namespace Game.Shared.PhysicsSystem.Subsystems
{
	public class DefaultGravity : IPhysicsSubsystem
	{
		private Vector3Wide gravityWideDt;
		public required Vector3 Gravity;

		public void Init( Simulation simulation )
		{
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void PrepareForIntegration( float dt )
		{
			// Precalculate gravity here
			gravityWideDt = Vector3Wide.Broadcast( Gravity * dt );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void IntegrateVelocity( ref BodyVelocityWide velocity )
		{
			velocity.Linear += gravityWideDt;
		}
	}
}
