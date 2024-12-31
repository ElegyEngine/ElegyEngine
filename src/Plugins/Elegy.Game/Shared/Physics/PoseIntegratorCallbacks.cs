// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using BepuPhysics;
using BepuUtilities;

namespace Game.Shared.Physics
{
	public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
	{
		/// <summary>
		/// Performs any required initialization logic after the Simulation instance has been constructed.
		/// </summary>
		public void Initialize( Simulation simulation )
		{
		}

		/// <summary>
		/// Gets how the pose integrator should handle angular velocity integration.
		/// </summary>
		public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

		/// <summary>
		/// Gets whether the integrator should use substepping for unconstrained bodies when using a substepping solver.
		/// If true, unconstrained bodies will be integrated with the same number of substeps as the constrained bodies in the solver.
		/// If false, unconstrained bodies use a single step of length equal to the dt provided to Simulation.Timestep. 
		/// </summary>
		public readonly bool AllowSubstepsForUnconstrainedBodies => false;

		/// <summary>
		/// Gets whether the velocity integration callback should be called for kinematic bodies.
		/// If true, IntegrateVelocity will be called for bundles including kinematic bodies.
		/// If false, kinematic bodies will just continue using whatever velocity they have set.
		/// Most use cases should set this to false.
		/// </summary>
		public readonly bool IntegrateVelocityForKinematics => false;

		public Vector3 Gravity;

		public PoseIntegratorCallbacks( Vector3 gravity ) : this()
		{
			Gravity = gravity;
		}

		//Note that velocity integration uses "wide" types. These are array-of-struct-of-arrays types that use SIMD accelerated types underneath.
		//Rather than handling a single body at a time, the callback handles up to Vector<float>.Count bodies simultaneously.
		Vector3Wide gravityWideDt;

		/// <summary>
		/// Callback invoked ahead of dispatches that may call into <see cref="IntegrateVelocity"/>.
		/// It may be called more than once with different values over a frame. For example, when performing bounding box prediction, velocity is integrated with a full frame time step duration.
		/// During substepped solves, integration is split into substepCount steps, each with fullFrameDuration / substepCount duration.
		/// The final integration pass for unconstrained bodies may be either fullFrameDuration or fullFrameDuration / substepCount, depending on the value of AllowSubstepsForUnconstrainedBodies. 
		/// </summary>
		public void PrepareForIntegration( float dt )
		{
			//No reason to recalculate gravity * dt for every body; just cache it ahead of time.
			gravityWideDt = Vector3Wide.Broadcast( Gravity * dt );
		}

		/// <summary>
		/// Callback for a bundle of bodies being integrated.
		/// </summary>
		public void IntegrateVelocity(
			Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia,
			Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity )
		{
			velocity.Linear += gravityWideDt;
		}
	}
}
