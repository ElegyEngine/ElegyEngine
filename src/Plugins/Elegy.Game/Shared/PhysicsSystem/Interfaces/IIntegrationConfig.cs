using BepuPhysics;

namespace Game.Shared.PhysicsSystem.Interfaces
{
	public interface IIntegrationConfig
	{
		AngularIntegrationMode AngularIntegrationMode { get; }
		bool AllowSubstepsForUnconstrainedBodies { get; }
		bool IntegrateVelocityForKinematics { get; }
	}
}
